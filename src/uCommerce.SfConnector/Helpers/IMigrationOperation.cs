using UConnector;

namespace uCommerce.SfConnector.Helpers
{
    public interface IMigrationOperation
    {
        IOperation BuildOperation();
    }
}
