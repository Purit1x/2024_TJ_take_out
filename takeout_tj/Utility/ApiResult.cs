namespace takeout_tj.Utility
{
    public class ApiResult<T> where T : class
    {
        /// <summary>
        /// Api执行是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// 结果集
        /// </summary>
        public T? Data { get; set; }
    }
}
