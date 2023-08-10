namespace SignalR.Redis.Host.Hubs
{
    /// <summary>
    /// 消息范围提供商
    /// </summary>
    public static class MessageScopingProviderName
    {
        /// <summary>
        /// 全局系统消息
        /// </summary>
        public const string System = "S";

        /// <summary>
        /// 系统
        /// </summary>
        public const string Application = "A";

        /// <summary>
        /// 组织机构/部门
        /// </summary>
        public const string Organization = "O";

        /// <summary>
        /// 角色
        /// </summary>
        public const string Role = "R";

        /// <summary>
        /// 群组
        /// </summary>
        public const string Group = "G";

        /// <summary>
        /// 用户
        /// </summary>
        public const string User = "U";
    }
}
