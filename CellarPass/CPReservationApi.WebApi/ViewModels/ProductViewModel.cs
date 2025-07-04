using CPReservationApi.Model;
using System.Collections.Generic;

namespace CPReservationApi.WebApi.ViewModels
{
    public class ShopItemsResponse : BaseResponse
    {
        public ShopItemsResponse()
        {
            data = new List<CollectionItem>();
        }
        public List<CollectionItem> data { get; set; }
    }

    public class StoreProductsResponse : BaseResponse
    {
        public StoreProductsResponse()
        {
            data = new List<StoreProduct>();
        }
        public List<StoreProduct> data { get; set; }
    }

    public class ProductDetailsResponse : BaseResponse
    {
        public ProductDetailsResponse()
        {
            data = new ProductDetails();
        }
        public ProductDetails data { get; set; }
    }

    public class ProductCollectionResponse : BaseResponse
    {
        public ProductCollectionResponse()
        {
            data = new List<ProductCollection>();
        }
        public List<ProductCollection> data { get; set; }
    }

    public class DepartmentResponse : BaseResponse
    {
        public DepartmentResponse()
        {
            data = new List<Department>();
        }
        public List<Department> data { get; set; }
    }

    public class AddUpdateCollectionResponse : BaseResponse
    {
        public AddUpdateCollectionResponse()
        {
            data = new ProductCollection();
        }
        public ProductCollection data { get; set; }
    }

    public class POSKeysResponse : BaseResponse
    {
        public POSKeysResponse()
        {
            data = new List<POSKey>();
        }
        public List<POSKey> data { get; set; }
    }

    public class AddUpdatePOSKeyResponse : BaseResponse
    {
        public AddUpdatePOSKeyResponse()
        {
            data = new POSKeyOut();
        }
        public POSKeyOut data { get; set; }
    }

    public class CollectionProductsResponse : BaseResponse
    {
        public CollectionProductsResponse()
        {
            data = new List<ProductDetails>();
        }
        public List<ProductDetails> data { get; set; }
    }

    public class RemovePOSKeyResponse : BaseResponse
    {
        public RemovePOSKeyResp data { get; set; }
    }

    public class UploadItemImageResponse : BaseResponse
    {
        public UploadItemImageResponse()
        {
            data = new AddItemImageResponseModel();
        }
        public AddItemImageResponseModel data { get; set; }
    }

    public class ProductsResponse : BaseResponse
    {
        public ProductsResponse()
        {
            data = new List<Product>();
        }
        public List<Product> data { get; set; }
    }
}
