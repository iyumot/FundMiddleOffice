using FMO.Utilities;

namespace FMO.IO.Trustee
{
    /// <summary>
    /// 托管接口
    /// </summary>
    public interface ITrusteeAssist : IDisposable
    {
        public  Task<bool> LoginAsync();

        /// <summary>
        /// 设置账号密码
        /// </summary>
        /// <returns></returns>
        public bool SetCredential(string name, string password)
        {
            using var db = new TrusteeDatabase();

            db.GetCollection<TrusteeCredential>().Upsert(new TrusteeCredential { Id = GetType().FullName!, Name = name, Password = password });

            return true;
        }
    }
}
