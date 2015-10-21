namespace JPB.Communication.Contracts.Security
{
    public interface ISecureMessageProvider
    {
        /// <summary>
        ///     Is used to convert the Complete Message Object into a byte[] that will be transferted to the remote computer
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        byte[] Encrypt(byte[] networkMessage);


        /// <summary>
        ///     Converts the output from the TCP network adapter into a valid TCP message
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        byte[] Decrypt(byte[] networkMessage);
    }
}
