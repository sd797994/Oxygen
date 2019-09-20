using System.Net;

namespace Oxygen.CommonTool
{
    /// <summary>
    /// 全局通用工具接口
    /// </summary>
    public interface IGlobalCommon
    {
        /// <summary>
        /// 获取可用端口号
        /// </summary>
        /// <returns></returns>
        int GetFreePort();
        /// <summary>
        /// 获取本机在局域网内的ip
        /// </summary>
        /// <returns></returns>
        IPAddress GetMachineIp();
        /// <summary>
        /// Rsa加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] RsaEncryp(byte[] data);
        /// <summary>
        /// Rsa解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] RsaDecrypt(byte[] data);


        /// <summary>
        /// BlowFish加密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] BfEncryp(byte[] data);
        /// <summary>
        /// BlowFish解密
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        byte[] BfDecrypt(byte[] data);


    }
}
