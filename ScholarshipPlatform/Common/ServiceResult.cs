namespace ScholarshipPlatform.Common;

public record ServiceResult<T> (bool IsSuccess, T? Data, Dictionary<string, string[]>? Errors)
{
    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>(true, data, null);
    }    

    public static ServiceResult<T> Failure(Dictionary<string, string[]> errors)
    {
        return new ServiceResult<T>(false, default, errors);
    }
}