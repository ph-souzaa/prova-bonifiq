namespace ProvaPub.Models.Responses
{
    public class Response<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static Response<T> Ok(T data, string message = "Operação realizada com sucesso")
        {
            return new Response<T> { Success = true, Message = message, Data = data };
        }

        public static Response<T> Fail(string message)
        {
            return new Response<T> { Success = false, Message = message, Data = default };
        }
    }
}
