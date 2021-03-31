public interface IData {

    public abstract void LoadResources();
    public abstract void ValidateData();
    public abstract string ConvertPath(string path);
}