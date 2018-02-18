namespace RapidPliant.Test
{
    public interface ITest
    {
        void Setup(bool benchmark = false);

        bool Run(bool benchmark = false);
    }
}