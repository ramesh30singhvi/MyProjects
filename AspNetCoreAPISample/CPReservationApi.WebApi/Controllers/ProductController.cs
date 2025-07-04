using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.DAL;
using CPReservationApi.Model;
using CPReservationApi.WebApi.Services;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using CPReservationApi.Common;

namespace CPReservationApi.WebApi.Controllers
{
    [Route("api/product")]
    public class ProductController : BaseController
    {
        public static IOptions<AppSettings> _appSetting;
        public ProductController(IOptions<AppSettings> appSetting) : base(appSetting)
        {
            _appSetting = appSetting;
        }

        [Route("shopitems")]
        [HttpGet]
        public IActionResult GetShopItems(int member_id, bool active_only = false)
        {
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            var productCategoryResponse = new ShopItemsResponse();

            try
            {
                var model = new List<CollectionItem>();

                model = productDAL.GetShopItems(member_id, active_only);

                if (model != null)
                {
                    productCategoryResponse.success = true;
                    productCategoryResponse.data = model;
                }
                else
                {
                    productCategoryResponse.success = true;
                    productCategoryResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    productCategoryResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                productCategoryResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                productCategoryResponse.error_info.extra_info = Common.Common.InternalServerError;
                productCategoryResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetShopItems:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(productCategoryResponse);
        }

        [Route("getallitems")]
        [HttpGet]
        public IActionResult GetAllItems(int member_id, bool active_only = false)
        {
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            var storeProductsResponse = new StoreProductsResponse();

            try
            {
                var model = new List<StoreProduct>();

                model = productDAL.GetAllItems(member_id, active_only);

                if (model != null)
                {
                    storeProductsResponse.success = true;
                    storeProductsResponse.data = model;
                }
                else
                {
                    storeProductsResponse.success = true;
                    storeProductsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    storeProductsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                storeProductsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                storeProductsResponse.error_info.extra_info = Common.Common.InternalServerError;
                storeProductsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetAllItems:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(storeProductsResponse);
        }

        [Route("itemdetails")]
        [HttpGet]
        public IActionResult GetItemDetails(int item_id)
        {
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            var productDetailsResponse = new ProductDetailsResponse();

            try
            {
                var model = new ProductDetails();

                model = productDAL.GetItemDetailsById(item_id);

                if (model != null)
                {
                    productDetailsResponse.success = true;
                    if (!string.IsNullOrWhiteSpace(model.item_image))
                    {
                        model.item_image = Path.Combine(StringHelpers.GetImagePath(ImageType.ProductImage, ImagePathType.azure), model.item_image);
                    }
                    productDetailsResponse.data = model;
                }
                else
                {
                    productDetailsResponse.success = true;
                    productDetailsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    productDetailsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                productDetailsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                productDetailsResponse.error_info.extra_info = Common.Common.InternalServerError;
                productDetailsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetItemDetails:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(productDetailsResponse);
        }

        [Route("removeitem")]
        [HttpPost]
        public IActionResult RemoveItem([FromBody]RemoveItemRequest model)
        {
            var resp = new RemovePOSKeyResponse();
            if (model == null || model.id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Id is required";
                resp.error_info.description = "Id is required";
                return new ObjectResult(resp);
            }

            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                resp.success = productDAL.RemoveItem(model.id);

                if (resp.success)
                {
                    RemovePOSKeyResp r = new RemovePOSKeyResp();
                    r.id = model.id;
                    resp.data = r;
                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "RemoveItem:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(resp);
        }

        [Route("addupdateitem")]
        [HttpPost]
        public IActionResult AddUpdateItem([FromBody]AddUpdateItemRequest model)
        {
            var resp = new RemovePOSKeyResponse();
            if (model == null || model.member_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Member Id is required";
                resp.error_info.description = "Member Id is required";
                return new ObjectResult(resp);
            }

            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                int productId = productDAL.AddUpdateItem(model);

                if (productId > 0)
                {
                    List<Variants> item_variants = new List<Variants>();

                    if (model.id > 0)
                        item_variants = productDAL.GetProductVariantsByProductId(productId);

                    foreach (var item in model.item_variant_ids)
                    {
                        int variant_id = 0;

                        if (item_variants.Count>0)
                            variant_id = item_variants.Where(t => t.variant_id == item).Select(t => t.variant_id).FirstOrDefault();

                        if (variant_id <= 0)
                            productDAL.InsertProductvariant(productId, item);
                    }

                    RemovePOSKeyResp r = new RemovePOSKeyResp();
                    r.id = productId;
                    resp.data = r;
                    resp.success = true;
                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AddUpdateItem:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(resp);
        }

        [Route("additemtocollection")]
        [HttpPost]
        public IActionResult AddItemToCollection([FromBody]RemoveItemFromCollectionRequest model)
        {
            var resp = new RemovePOSKeyResponse();
            if (model == null || model.item_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Item Id is required";
                resp.error_info.description = "Item Id is required";
                return new ObjectResult(resp);
            }

            if (model == null || model.collection_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Collection Id is required";
                resp.error_info.description = "Collection Id is required";
                return new ObjectResult(resp);
            }

            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                if (productDAL.IsValidCollectionAndProductId(model.item_id, model.collection_id))
                {
                    bool insertval = true;

                    string strCollections = productDAL.GetCollectionsByProductId(model.item_id);

                    if (strCollections.Length > 0)
                    {
                        List<int> listcollections = strCollections.Split(',').Select(int.Parse).ToList();

                        var item_id = listcollections.Where(t => t == model.collection_id).Select(t => t).FirstOrDefault();

                        if (item_id <= 0)
                            strCollections = strCollections + ",";
                        else
                            insertval = false;
                    }
                        
                    if (insertval)
                    {
                        strCollections = strCollections + model.collection_id.ToString();

                        resp.success = productDAL.UpdateItemToCollection(model.item_id, strCollections);
                    }

                    if (resp.success)
                    {
                        RemovePOSKeyResp r = new RemovePOSKeyResp();
                        r.id = model.item_id;
                        resp.data = r;
                    }
                }
                else
                {
                    resp.success = false;
                    resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                    resp.error_info.extra_info = "Collection Id is Invalid";
                    resp.error_info.description = "Collection Id is Invalid";
                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AddItemToCollection:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(resp);
        }

        [Route("removeitemfromcollection")]
        [HttpPost]
        public IActionResult RemoveItemFromCollection([FromBody]RemoveItemFromCollectionRequest model)
        {
            var resp = new RemovePOSKeyResponse();
            if (model == null || model.item_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Item Id is required";
                resp.error_info.description = "Item Id is required";
                return new ObjectResult(resp);
            }

            if (model == null || model.collection_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Collection Id is required";
                resp.error_info.description = "Collection Id is required";
                return new ObjectResult(resp);
            }

            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                string strCollections = productDAL.GetCollectionsByProductId(model.item_id);

                List<int> listcollections = strCollections.Split(',').Select(int.Parse).ToList();

                strCollections = "";

                foreach (var item in listcollections)
                {
                    if (item != model.collection_id)
                    {
                        if (strCollections.Length > 0)
                            strCollections = strCollections + ",";

                        strCollections = strCollections + item.ToString();
                    }
                }

                resp.success = productDAL.UpdateItemToCollection(model.item_id, strCollections);

                if (resp.success)
                {
                    RemovePOSKeyResp r = new RemovePOSKeyResp();
                    r.id = model.item_id;
                    resp.data = r;
                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "RemoveItemFromCollection:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(resp);
        }

        [Route("collections")]
        [HttpGet]
        public IActionResult GetCollectionList(int member_id)
        {
            var collectionResponse = new ProductCollectionResponse();
            if (member_id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Invalid Member Id";
                collectionResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(collectionResponse);
            }
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                var model = new List<ProductCollection>();

                model = productDAL.GetCollectionListByMember(member_id);

                if (model != null)
                {
                    collectionResponse.success = true;
                    collectionResponse.data = model;
                }
                else
                {
                    collectionResponse.success = true;
                    collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    collectionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                collectionResponse.error_info.extra_info = Common.Common.InternalServerError;
                collectionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetCollectionList:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(collectionResponse);
        }

        [Route("addcollection")]
        [HttpPost]
        public IActionResult AddCollection([FromBody]ProductCollection model)
        {
            var collectionResponse = new AddUpdateCollectionResponse();
            if (model == null || string.IsNullOrWhiteSpace(model.collection_name))
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Collection name is required";
                collectionResponse.error_info.description = "Collection name is required";
                return new ObjectResult(collectionResponse);
            }
            if (model.member_id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Invalid Member Id";
                collectionResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(collectionResponse);
            }
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                var response = new ProductCollection();

                response = productDAL.AddUpdateCollection(model);

                if (model != null)
                {
                    collectionResponse.success = true;
                    collectionResponse.data = response;
                }
                else
                {
                    collectionResponse.success = false;
                    collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    collectionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                collectionResponse.error_info.extra_info = Common.Common.InternalServerError;
                collectionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AddCollection:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(collectionResponse);
        }

        [Route("updatecollection")]
        [HttpPost]
        public IActionResult UpdateCollection([FromBody]ProductCollection model)
        {
            var collectionResponse = new AddUpdateCollectionResponse();
            if (model == null || string.IsNullOrWhiteSpace(model.collection_name))
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Collection name is required";
                collectionResponse.error_info.description = "Collection name is required";
                return new ObjectResult(collectionResponse);
            }
            if (model.member_id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Invalid Member Id";
                collectionResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(collectionResponse);
            }
            if (model.id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Invalid Collection Id";
                collectionResponse.error_info.description = "Invalid Collection Id";
                return new ObjectResult(collectionResponse);
            }
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                var response = new ProductCollection();

                response = productDAL.AddUpdateCollection(model);

                if (model != null)
                {
                    collectionResponse.success = true;
                    collectionResponse.data = response;
                }
                else
                {
                    collectionResponse.success = false;
                    collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    collectionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                collectionResponse.error_info.extra_info = Common.Common.InternalServerError;
                collectionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdateCollection:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(collectionResponse);
        }

        [Route("poskeyslist")]
        [HttpGet]
        public IActionResult GetPOSKeysList(int member_id)
        {
            var collectionResponse = new POSKeysResponse();
            if (member_id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Invalid Member Id";
                collectionResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(collectionResponse);
            }
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                var model = new List<POSKey>();

                model = productDAL.GetPOSKeyListByMember(member_id);

                if (model != null)
                {
                    collectionResponse.success = true;
                    collectionResponse.data = model;
                }
                else
                {
                    collectionResponse.success = true;
                    collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    collectionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                collectionResponse.error_info.extra_info = Common.Common.InternalServerError;
                collectionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetCollectionList:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,member_id);
            }
            return new ObjectResult(collectionResponse);
        }

        [Route("addposkey")]
        [HttpPost]
        public IActionResult AddPOSKey([FromBody]POSKey model)
        {
            var collectionResponse = new AddUpdatePOSKeyResponse();
            if (model == null || model.product_id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Product Id is required";
                collectionResponse.error_info.description = "Product Id is required";
                return new ObjectResult(collectionResponse);
            }
            if (model.member_id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Invalid Member Id";
                collectionResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(collectionResponse);
            }
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                var response = new POSKeyOut();

                response = productDAL.AddUpdatePOSKey(model);

                if (model != null)
                {
                    collectionResponse.success = true;
                    collectionResponse.data = response;
                }
                else
                {
                    collectionResponse.success = false;
                    collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    collectionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                collectionResponse.error_info.extra_info = Common.Common.InternalServerError;
                collectionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "AddPOSKey:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(collectionResponse);
        }

        [Route("updateposkey")]
        [HttpPost]
        public IActionResult UpdatePOSKey([FromBody]POSKey model)
        {
            var collectionResponse = new AddUpdatePOSKeyResponse();
            if (model == null || model.product_id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Product Id is required";
                collectionResponse.error_info.description = "Product Id is required";
                return new ObjectResult(collectionResponse);
            }
            if (model.member_id <= 0)
            {
                collectionResponse.success = false;
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                collectionResponse.error_info.extra_info = "Invalid Member Id";
                collectionResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(collectionResponse);
            }
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                var response = new POSKeyOut();

                response = productDAL.AddUpdatePOSKey(model);

                if (model != null)
                {
                    collectionResponse.success = true;
                    collectionResponse.data = response;
                }
                else
                {
                    collectionResponse.success = false;
                    collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.None;
                    collectionResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                collectionResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                collectionResponse.error_info.extra_info = Common.Common.InternalServerError;
                collectionResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UpdatePOSKey:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(collectionResponse);
        }

        [Route("removeposkey")]
        [HttpPost]
        public IActionResult RemovePOSKey([FromBody]RemovePOSKeyRequest model)
        {
            var resp = new RemovePOSKeyResponse();
            if (model == null || model.id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Id is required";
                resp.error_info.description = "Id is required";
                return new ObjectResult(resp);
            }

            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                resp.success = productDAL.RemovePOSKey(model.id);

                if (resp.success)
                {
                    RemovePOSKeyResp r = new RemovePOSKeyResp();
                    r.id = model.id;
                    resp.data = r;
                }
            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "RemovePOSKey:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(resp);
        }

        [Route("removecollection")]
        [HttpPost]
        public IActionResult RemoveCollection([FromBody]RemoveCollectionRequest model)
        {
            var resp = new RemovePOSKeyResponse();
            if (model == null || model.id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Id is required";
                resp.error_info.description = "Id is required";
                return new ObjectResult(resp);
            }

            if (model == null || model.member_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Member Id is required";
                resp.error_info.description = "Member Id is required";
                return new ObjectResult(resp);
            }

            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                resp.success = productDAL.RemoveCollection(model.id, model.member_id);

                if (resp.success)
                {
                    RemovePOSKeyResp r = new RemovePOSKeyResp();
                    r.id = model.id;
                    resp.data = r;
                }

            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "RemoveCollection:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,model.member_id);
            }
            return new ObjectResult(resp);
        }

        [Route("getproductsbycollection")]
        [HttpGet]
        public IActionResult GetProductsByCollection(int collection_id)
        {
            var productsResponse = new CollectionProductsResponse();

            if (collection_id <= 0)
            {
                productsResponse.success = false;
                productsResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                productsResponse.error_info.extra_info = "Collection Id not found";
                productsResponse.error_info.description = "Collection Id not found";
                return new ObjectResult(productsResponse);
            }
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);
            try
            {
                var model = new List<ProductDetails>();

                model = productDAL.GetCollectionProductsById(collection_id);

                if (model != null)
                {
                    productsResponse.success = true;
                    productsResponse.data = model;
                }
                else
                {
                    productsResponse.success = true;
                    productsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    productsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                productsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                productsResponse.error_info.extra_info = Common.Common.InternalServerError;
                productsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetProductsByCollection:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(productsResponse);
        }

        [Route("uploaditemimage")]
        [HttpPost, DisableRequestSizeLimit]
        public IActionResult UploadItemImage([FromForm]int item_id)
        {
            var resp = new UploadItemImageResponse();
            if (item_id <= 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "Id is required";
                resp.error_info.description = "Id is required";
                return new ObjectResult(resp);
            }
            var file = Request.Form.Files[0];
            if (file == null || file.Length == 0)
            {
                resp.success = false;
                resp.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                resp.error_info.extra_info = "No item image file found";
                resp.error_info.description = "No item image file found";
                return new ObjectResult(resp);
            }

            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                string fileName = string.Format("product-{0}.jpg", item_id);
                string itemimageURL = "";
                using (var memoryStream = new MemoryStream())
                {
                    file.OpenReadStream().CopyTo(memoryStream);
                    byte[] bytes = memoryStream.ToArray();
                    itemimageURL = Utility.UploadFileToStorage(bytes, fileName, ImageType.ProductImage);
                }
                bool isSuccess = productDAL.UpdateItemImageURL(item_id, fileName);

                if (isSuccess)
                {
                    resp.data.item_image_url = itemimageURL;
                }
                resp.success = isSuccess;

            }
            catch (Exception ex)
            {
                resp.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                resp.error_info.extra_info = Common.Common.InternalServerError;
                resp.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "UploadItemImage:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"],1,0);
            }
            return new ObjectResult(resp);
        }

        [Route("searchproducts")]
        [HttpGet]
        public IActionResult GetProductsBykeyboard(string keyword,int member_id, bool active_only = false)
        {
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            var productsResponse = new ProductsResponse();

            try
            {
                var model = new List<Product>();

                model = productDAL.GetProductsBykeyboard(keyword,member_id, active_only);

                if (model != null)
                {
                    productsResponse.success = true;
                    productsResponse.data = model;
                }
                else
                {
                    productsResponse.success = true;
                    productsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    productsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                productsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                productsResponse.error_info.extra_info = Common.Common.InternalServerError;
                productsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetProductsBykeyboard" +
                    ":  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(productsResponse);
        }

        [Route("bigcommercesearchproducts")]
        [HttpGet]
        public async Task<IActionResult> GetBigCommerceProductsBykeyboard(string keyword, int member_id)
        {
            var productsResponse = new BigCommerceProductsResponse();

            try
            {
                EventDAL eventDAL = new EventDAL(Common.Common.ConnectionString);
                WineryModel memberModel = await Task.Run(() => eventDAL.GetWineryById(member_id));

                if (memberModel.EnableClubBigCommerce && !string.IsNullOrWhiteSpace(memberModel.BigCommerceAceessToken) && !string.IsNullOrWhiteSpace(memberModel.BigCommerceStoreId))
                {
                    ProductList list = await Task.Run(() => Utility.GetBigCommerceProducts(memberModel.BigCommerceStoreId, memberModel.BigCommerceAceessToken, keyword, member_id));

                    if (list != null && list.data != null && list.data.Count > 0)
                        productsResponse.data = list.data;
                }

                if (productsResponse.data.Count == 0)
                {
                    productsResponse.success = true;
                    productsResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    productsResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                productsResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                productsResponse.error_info.extra_info = Common.Common.InternalServerError;
                productsResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetBigCommerceProductsBykeyboard" +
                    ":  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(productsResponse);
        }

        [Route("departments")]
        [HttpGet]
        public IActionResult GetDepartments(int member_id)
        {
            var departmentResponse = new DepartmentResponse();
            if (member_id <= 0)
            {
                departmentResponse.success = false;
                departmentResponse.error_info.error_type = (int)Common.Common.ErrorType.InvalidData;
                departmentResponse.error_info.extra_info = "Invalid Member Id";
                departmentResponse.error_info.description = "Invalid Member Id";
                return new ObjectResult(departmentResponse);
            }
            ProductDAL productDAL = new ProductDAL(Common.Common.ConnectionString);

            try
            {
                var model = new List<Department>();

                model = productDAL.GetDepartments(member_id);

                if (model != null)
                {
                    departmentResponse.success = true;
                    departmentResponse.data = model;
                }
                else
                {
                    departmentResponse.success = true;
                    departmentResponse.error_info.error_type = (int)Common.Common.ErrorType.NoRecordFound;
                    departmentResponse.error_info.extra_info = "no record found";
                }
            }
            catch (Exception ex)
            {
                departmentResponse.error_info.error_type = (int)Common.Common.ErrorType.Exception;
                departmentResponse.error_info.extra_info = Common.Common.InternalServerError;
                departmentResponse.error_info.description = ex.Message.ToString();
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("WebApi", "GetDepartments:  " + ex.Message.ToString(), HttpContext.Request.Headers["AuthenticateKey"], 1, member_id);
            }
            return new ObjectResult(departmentResponse);
        }
    }
}