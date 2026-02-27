namespace Spendly.Shared.ViewModels;

public class FunctionResponse<T> : FunctionResponse
{
    public T? Data { get; }

    private FunctionResponse(T? data, bool isSuccess, string? errorMessage) 
       : base(isSuccess, errorMessage)
    {
       Data = data;
    }

    public static FunctionResponse<T> Success(T data) 
       => new FunctionResponse<T>(data, true, null);

    public static FunctionResponse<T> Failure(string errorMessage, T? data = default) 
       => new FunctionResponse<T>(data, false, errorMessage);
}

public class FunctionResponse(bool isSuccess, string? errorMessage, List<string>? messageParameters = null)
{
    public bool IsSuccess { get; } = isSuccess;
    public string? ErrorMessage { get; } = errorMessage;
    public List<string>? MessageParameters { get; set; } = messageParameters;

    public static FunctionResponse Success() 
       => new FunctionResponse(true, null);
    
    public static FunctionResponse Failure(string errorMessage, List<string>? messageParameters = null) 
       => new FunctionResponse(false, errorMessage, messageParameters);

    // Generic helpers to enable type inference at call sites
    public static FunctionResponse<T> Success<T>(T data) 
       => FunctionResponse<T>.Success(data);

    public static FunctionResponse<T> Failure<T>(string errorMessage, T? data = default) 
       => FunctionResponse<T>.Failure(errorMessage, data);
}
