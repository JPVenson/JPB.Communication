/*
 Created by Jean-Pierre Bachmann
 Visit my GitHub page at:
 
 https://github.com/JPVenson/

 Please respect the Code and Work of other Programers an Read the license carefully

 GNU AFFERO GENERAL PUBLIC LICENSE
                       Version 3, 19 November 2007

 Copyright (C) 2007 Free Software Foundation, Inc. <http://fsf.org/>
 Everyone is permitted to copy and distribute verbatim copies
 of this license document, but changing it is not allowed.

 READ THE FULL LICENSE AT:

 https://github.com/JPVenson/JPB.Communication/blob/master/LICENSE
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.Contracts;
using JPB.Communication.Shared.CrossPlatform;

namespace JPB.Communication.WinRT.Serilizer
{
    /// <summary>
    ///     Contains a Mixed Message Serlilizer that Converts the Message as XML and the Content to Binary
    ///     Is able to work with objects that are in the same namespace but in diferent Assemablys like when using ILMerge
    ///     After 20 mb of message size the content of the message will be paged to the disc if PreventDiscPageing is not set to true
    /// </summary>
    public class DefaultMessageSerlilizer : IMessageSerializer
    {
        public DefaultMessageSerlilizer()
        {
            IlMergeSupport = true;
            _binder = new IlMergeBinder();
        }
        //IL MERGE OR RENAMING SUPPORT

        internal static Encoding Encoding = Encoding.ASCII;
        private IlMergeBinder _binder;
        public bool IlMergeSupport { get; set; }
        public bool PrevendDiscPageing { get; set; }

        public byte[] SerializeMessage(object mess)
        {
            if (IlMergeSupport)
            {
                //what goes out maybe comes again in
                _binder.AddOptimistic(mess.GetType());
            }

            //support for large objects
            using (Stream stream = GetStream(mess))
            {
                CreateFormatter().WriteObject(stream, mess);
                return getInfo(stream);
            }
        }


        public object DeSerializeMessage(byte[] source)
        {
            try
            {
                using (var memst = new MemoryStream(source))
                {
                    var deserialize = CreateFormatter().ReadObject(memst);
                    return deserialize;
                }
            }
            catch (Exception e)
            {
                PclTrace.WriteLine(e.ToString(), Networkbase.TraceCategoryCriticalSerilization);
                return null;
            }
        }

        public string ResolveStringContent(byte[] message)
        {
            return (Encoding.GetString(message, 0, message.Length));
        }

        private Stream GetStream(object validator)
        {
            string target = string.Empty;
            Stream stream = null;

            if (!PrevendDiscPageing &&
                (validator is Array &&
                 (validator as Array).LongLength > (InternalMemoryHolder.MaximumStoreageInMemory * 4)))
            {
                target = Path.GetTempFileName();
                stream = new FileStream(target, FileMode.Open);
            }
            else
            {
                stream = new MemoryStream();
            }
            return stream;
        }

        private byte[] getInfo(Stream stream)
        {
            var array = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(array, 0, array.Length);
            return array;
        }

        private DataContractSerializer CreateFormatter()
        {
            var settings = new DataContractSerializerSettings();
            settings.DataContractResolver = new IlMergeContractResolverFasade();
            settings.IgnoreExtensionDataObject = false;
            settings.PreserveObjectReferences = true;
            settings.SerializeReadOnlyTypes = false;
            var formartter = new DataContractSerializer(typeof(MessageBase), settings);

            return formartter;
        }

        public class IlMergeContractResolverFasade : DataContractResolver
        {
            IlMergeBinder @base;
            public IlMergeContractResolverFasade()
            {
                @base = new IlMergeBinder();
            }
            public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
            {
                return @base.BindToType(declaredType.Assembly.FullName, typeName);
            }

            public override bool TryResolveType(Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
            {
                var name = type.Name;
                var namesp = type.Namespace;
                typeName = new XmlDictionaryString(XmlDictionary.Empty, name, 0);
                typeNamespace = new XmlDictionaryString(XmlDictionary.Empty, namesp, 0);
                var foundElement = @base.BindToType(type.Assembly.FullName, name);
                return foundElement != null;
            }
        }

        internal sealed class IlMergeBinder : SerializationBinder
        {
            private static readonly string MessageBaseTypeName;
            private static readonly List<AssemblyName> AdditionalLookups;
            private static ConcurrentDictionary<string, Type> TypnameToType { get; set; }

            static IlMergeBinder()
            {
                TypnameToType = new ConcurrentDictionary<string, Type>();
                MessageBaseTypeName = typeof(MessageBase).FullName;
                AdditionalLookups = new List<AssemblyName>();
            }

            public static Dictionary<string, Type> GetOptimistics()
            {
                return TypnameToType.ToDictionary(s => s.Key, s => s.Value);
            }

            private static Type TryAdd(string alias, Type type)
            {
                //dynamic binding 
                lock (type)
                {
                    return TypnameToType.GetOrAdd(alias, type);
                }
            }

            public override Type BindToType(string assemblyName, string typeName)
            {
                //Native support for MessageBase
                if (MessageBaseTypeName == typeName)
                {
                    return typeof(MessageBase);
                }

                //Optimistic Serach
                if (TypnameToType.ContainsKey(typeName))
                {
                    return TypnameToType[typeName];
                }

                Type foundType = null;

                //if an assembly name is provided try to load the assembly or search for it in the known assemblys

                var currentAssemblyes = AppDomain.CurrentDomain.GetAssemblies();

                if (!string.IsNullOrEmpty(assemblyName))
                {
                    var specialName = currentAssemblyes.FirstOrDefault(s => s.FullName == assemblyName);
                    if (specialName != null)
                    {
                        foundType = specialName.DefinedTypes.FirstOrDefault(s => s.Name.Equals(typeName));
                        if (foundType != null)
                        {
                            return TryAdd(typeName, foundType);
                        }
                    }
                }


                foreach (var currentAssemblye in currentAssemblyes)
                {
                    foundType = currentAssemblye.DefinedTypes.FirstOrDefault(s => s.Name.Equals(typeName));
                    if (foundType != null)
                    {
                        return TryAdd(typeName, foundType);
                    }
                }

                //Search throu all Referenced and not loaded assemblys
                Assembly callingAssembly = Assembly.GetEntryAssembly();
                Assembly current = Assembly.GetExecutingAssembly();
                foundType = callingAssembly.GetReferencedAssemblies().Concat(new[]
                {
                    current.GetName(),
                    callingAssembly.GetName()
                })
                    .Select(Assembly.Load)
                    .Select(assembly => assembly.GetType(typeName)).FirstOrDefault(type => type != null);

                if (foundType != null)
                {
                    return TryAdd(typeName, foundType);
                }
                return null;
            }

            public void AddOptimistic(Type type)
            {
                AddOptimistic(type);
            }

            public static void GlobalAddOptimistic(Type type)
            {
                string fullName = type.FullName;
                if (!TypnameToType.ContainsKey(fullName) && !fullName.Contains("System") &&
                    !fullName.Contains("mscorlib"))
                {
                    TryAdd(type.FullName, type);
                }
            }
        }
    }
}