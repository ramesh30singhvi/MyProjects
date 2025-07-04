using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using uc = CPReservationApi.Common;

namespace CPReservationApi.DAL
{
    public class ProductDAL : BaseDataAccess
    {
        public ProductDAL(string connectionString) : base(connectionString)
        {
        }

        public List<CollectionItem> GetShopItems(int WineryID, bool active_only)
        {
            var collectionItems = new List<CollectionItem>();
            var productCategory = new CollectionItem();

            string sql = "GetShopItemsByMember";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryID", WineryID));

            int OldCollectionId = -1;

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        int CollectionId = Convert.ToInt32(dataReader["CollectionId"]);

                        if (OldCollectionId == -1 || OldCollectionId != CollectionId)
                        {
                            productCategory = new CollectionItem();
                            productCategory.items = new List<Product>();

                            OldCollectionId = CollectionId;

                            productCategory.id = Convert.ToInt32(dataReader["CollectionId"]);

                            productCategory.collection_name = Convert.ToString(dataReader["CollectionName"]);
                            productCategory.sort_order = Convert.ToInt32(dataReader["SortOrder"]);
                            collectionItems.Add(productCategory);
                        }

                        var items = new Product();

                        items.id = Convert.ToInt32(dataReader["Id"]);

                        if (Convert.ToString(dataReader["ImageURL"]).Length > 0)
                            items.image = string.Format("{0}{1}/{2}", "https://cdncellarpass.blob.core.windows.net", "/photos/product_images", Convert.ToString(dataReader["ImageURL"]));

                        items.status = Convert.ToInt16(dataReader["Status"]);

                        if (items.status == 1 || items.status == 2)
                            items.is_active = true;
                        else
                            items.is_active = false;

                        if (items.status == 1)
                            items.is_public = true;
                        else
                            items.is_public = false;

                        items.name = Convert.ToString(dataReader["ProductName"]);
                        items.sku = Convert.ToString(dataReader["Sku"]);
                        items.retail_price = Convert.ToDecimal(dataReader["RetailPrice"]);
                        items.charge_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        items.custom_field_1 = Convert.ToString(dataReader["CustomField1"]);
                        items.custom_field_2 = Convert.ToString(dataReader["CustomField2"]);
                        items.department_id = Convert.ToInt32(dataReader["DepartmentId"]);
                        string tags = Convert.ToString(dataReader["Tags"]);

                        items.tags = tags.Split(',').ToList();

                        int WineCategory = Convert.ToInt32(dataReader["WineCategory"]);

                        items.category = uc.Common.GetWineCategory().Where(f => f.ID == WineCategory.ToString()).Select(f => f.Name).FirstOrDefault();

                        if (active_only)
                        {
                            if (items.is_active)
                                productCategory.items.Add(items);
                        }
                        else
                            productCategory.items.Add(items);
                    }
                }
            }
            return collectionItems;
        }

        public List<Product> GetProductsBykeyboard(string keyword, int WineryID, bool active_only)
        {
            var products = new List<Product>();

            string sql = "GetProductsBykeyboard";

            var parameterList = new List<DbParameter>();

            string WhereClause = string.Format(" where wineryid={0} and (Sku like '{1}%' or ProductName like '{1}%' or isnull(CustomField1,'') like '{1}%' or isnull(CustomField2,'') like '{1}%' or isnull(Tags,'') like '{1}%' or isnull(Tags,'') like '%,{1}%'  or isnull(Tags,'') like '%, {1}%')", WineryID, keyword);
            parameterList.Add(GetParameter("@WhereClause", WhereClause));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var items = new Product();

                        items.id = Convert.ToInt32(dataReader["Id"]);

                        if (Convert.ToString(dataReader["ImageURL"]).Length > 0)
                            items.image = string.Format("{0}{1}/{2}", "https://cdncellarpass.blob.core.windows.net", "/photos/product_images", Convert.ToString(dataReader["ImageURL"]));

                        items.status = Convert.ToInt16(dataReader["Status"]);

                        if (items.status == 1 || items.status == 2)
                            items.is_active = true;
                        else
                            items.is_active = false;

                        if (items.status == 1)
                            items.is_public = true;
                        else
                            items.is_public = false;

                        items.name = Convert.ToString(dataReader["ProductName"]);
                        items.sku = Convert.ToString(dataReader["Sku"]);
                        items.retail_price = Convert.ToDecimal(dataReader["RetailPrice"]);
                        items.charge_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        items.custom_field_1 = Convert.ToString(dataReader["CustomField1"]);
                        items.custom_field_2 = Convert.ToString(dataReader["CustomField2"]);
                        string tags = Convert.ToString(dataReader["Tags"]);

                        items.tags = tags.Split(',').ToList();

                        int WineCategory = Convert.ToInt32(dataReader["WineCategory"]);

                        items.category = uc.Common.GetWineCategory().Where(f => f.ID == WineCategory.ToString()).Select(f => f.Name).FirstOrDefault();

                        if (active_only)
                        {
                            if (items.is_active)
                                products.Add(items);
                        }
                        else
                            products.Add(items);
                    }
                }
            }
            return products;
        }

        public List<StoreProduct> GetAllItems(int WineryID, bool active_only)
        {
            var storeProducts = new List<StoreProduct>();

            string sql = "GetAllProductsByWineryId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@WineryId", WineryID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var items = new StoreProduct();

                        items.id = Convert.ToInt32(dataReader["Id"]);

                        items.status = Convert.ToInt16(dataReader["Status"]);

                        if (items.status == 1 || items.status == 2)
                            items.active = true;
                        else
                            items.active = false;

                        items.name = Convert.ToString(dataReader["ProductName"]);
                        items.sku = Convert.ToString(dataReader["Sku"]);
                        items.retail_price = Convert.ToDecimal(dataReader["RetailPrice"]);

                        int variantCount = GetProductVariantsByProductId(items.id).Count;

                        if (variantCount > 0)
                            variantCount += 1;

                        items.variant = variantCount.ToString() + " Variants";

                        int WineCategory = Convert.ToInt32(dataReader["WineCategory"]);

                        items.category = uc.Common.GetWineCategory().Where(f => f.ID == WineCategory.ToString()).Select(f => f.Name).FirstOrDefault();

                        if (active_only)
                        {
                            if (items.active)
                                storeProducts.Add(items);
                        }
                        else
                            storeProducts.Add(items);
                    }
                }
            }
            return storeProducts;
        }

        public List<string> GetCollectionList(string strCollections)
        {
            List<string> list = new List<string>();

            List<int> listcollections = strCollections.Split(',').Select(int.Parse).ToList();

            foreach (var item in listcollections)
            {
                string sql = "select CollectionName from Collection_Item where id = @Id";

                var parameterList = new List<DbParameter>();
                parameterList.Add(GetParameter("@Id", item));

                using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
                {
                    if (dataReader != null && dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            list.Add(Convert.ToString(dataReader["CollectionName"]));
                        }
                    }
                }
            }

            return list;
        }


        public ProductDetails GetItemDetailsById(int ProductID)
        {
            var model = new ProductDetails();

            string sql = "GetItemDetailsById";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", ProductID));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        model.id = ProductID;

                        model.item_image = Convert.ToString(dataReader["ImageURL"]);

                        model.status = Convert.ToInt16(dataReader["Status"]);

                        if (model.status == 1 || model.status == 2)
                            model.is_active = true;
                        else
                            model.is_active = false;

                        if (model.status == 1)
                            model.is_public = true;
                        else
                            model.is_public = false;

                        model.item_name = Convert.ToString(dataReader["ProductName"]);
                        model.item_sku = Convert.ToString(dataReader["Sku"]);
                        model.retail_price = Convert.ToDecimal(dataReader["RetailPrice"]);

                        model.charge_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        model.club_price = Convert.ToDecimal(dataReader["ClubPrice"]);

                        model.cost = Convert.ToDecimal(dataReader["Cost"]);
                        model.custom_field_1 = Convert.ToString(dataReader["CustomField1"]);
                        model.custom_field_2 = Convert.ToString(dataReader["CustomField2"]);
                        model.display_in_pos = Convert.ToBoolean(dataReader["DisplayinPOS"]);
                        model.fulfillment_service = Convert.ToString(dataReader["FulfillmentService"]);
                        model.inventory_mode = Convert.ToInt32(dataReader["InventoryPolicy"]);
                        model.in_stock_quantity = Convert.ToInt32(dataReader["InventoryQuantity"]);
                        model.item_category = Convert.ToInt32(dataReader["WineCategory"]);
                        model.item_desc = Convert.ToString(dataReader["Description"]);
                        model.department_id = Convert.ToInt32(dataReader["DepartmentId"]);

                        ItemDimensions itemDimensions = new ItemDimensions();

                        itemDimensions.height = Convert.ToDecimal(dataReader["Height"]);
                        itemDimensions.length = Convert.ToDecimal(dataReader["Length"]);
                        itemDimensions.width = Convert.ToDecimal(dataReader["Width"]);

                        model.item_dimensions = itemDimensions;

                        model.item_weight = Convert.ToDecimal(dataReader["ItemWeight"]);
                        model.requires_shipping = Convert.ToBoolean(dataReader["RequiresShipping"]);
                        model.sale_price = Convert.ToDecimal(dataReader["SalePrice"]);

                        string tags = Convert.ToString(dataReader["Tags"]);

                        model.tags = tags.Split(',').ToList();

                        string collections = Convert.ToString(dataReader["Collections"]);

                        if (collections.Length > 0)
                            model.collections = GetCollectionList(collections);

                        model.unit_type = Convert.ToInt32(dataReader["UnitType"]);
                        model.volume = Convert.ToString(dataReader["Volume"]);
                        model.weight_option = Convert.ToInt32(dataReader["Weightoption"]);
                        model.exclude_from_discounts = Convert.ToBoolean(dataReader["ExcludefromDiscounts"]);
                        model.external_url = Convert.ToString(dataReader["ExternalURL"]);
                        model.vendor_id = Convert.ToInt32(dataReader["VendorId"]);
                        model.product_template = Convert.ToInt32(dataReader["ProductTemplate"]);
                        model.meta_tax_type = Convert.ToInt32(dataReader["MetaTaxType"]);
                        model.meta_item_type = Convert.ToInt32(dataReader["MetaItemType"]);
                        model.meta_brand_key = Convert.ToString(dataReader["MetaBrandKey"]);
                        model.meta_product_key = Convert.ToString(dataReader["MetaProductKey"]);
                        model.access_and_security = Convert.ToInt32(dataReader["AccessAndSecurityId"]);
                        model.image_url = Convert.ToString(dataReader["ImageURL"]);
                        var accessSecurityTags = Convert.ToString(dataReader["AccessSecurityTags"]);
                        if (!String.IsNullOrEmpty(accessSecurityTags))
                        {
                            model.access_security_tags = accessSecurityTags.Split(',').Select(x => int.Parse(x.Trim())).ToList();
                        }
                        var accessSecurityClubs = Convert.ToString(dataReader["AccessSecurityClubs"]);
                        if (!String.IsNullOrEmpty(accessSecurityClubs))
                        {
                            model.access_security_clubs = accessSecurityClubs.Split(',').Select(x => int.Parse(x.Trim())).ToList();
                        }

                        model.access_security_name = Convert.ToString(dataReader["AccessName"]);
                        model.department_name = Convert.ToString(dataReader["DepartmentName"]);
                        model.vendor_name = Convert.ToString(dataReader["VendorName"]);
                        model.template_name = Convert.ToString(dataReader["TemplateName"]);
                        model.tax_class_name = Convert.ToString(dataReader["TaxClassName"]);

                        List<Variants> variants = new List<Variants>();
                        if (dataReader.NextResult())
                        {
                            while (dataReader.Read())
                            {
                                var v = new Variants
                                {
                                    id = Convert.ToInt32(dataReader["Id"]),
                                    name = Convert.ToString(dataReader["ProductName"]),
                                    sku = Convert.ToString(dataReader["SKU"]),
                                    retail_price = Convert.ToDecimal(dataReader["RetailPrice"])
                                };
                                variants.Add(v);
                            }
                        }

                        model.item_variants = variants;
                    }
                }
            }
            return model;
        }

        public List<Variants> GetProductVariantsByProductId(int productId)
        {
            var variants = new List<Variants>();

            string sql = "GetProductVariantsByProductId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", productId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var v = new Variants
                        {
                            id = Convert.ToInt32(dataReader["ProductId"]),
                            name = Convert.ToString(dataReader["ProductName"]),
                            sku = Convert.ToString(dataReader["SKU"]),
                            retail_price = Convert.ToDecimal(dataReader["RetailPrice"]),
                            variant_id = Convert.ToInt32(dataReader["variantId"]),
                        };
                        variants.Add(v);
                    }
                }
            }

            return variants;
        }

        public string GetCollectionsByProductId(int productId)
        {
            string Collections = "";
            string sql = "select Collections from product where id=@Id";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", productId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        Collections = Convert.ToString(dataReader["Collections"]);
                    }
                }
            }

            return Collections;
        }

        public bool IsValidCollectionAndProductId(int productId, int CollectionId)
        {
            bool ret = false;
            string sql = "select P.Id from product p join [Collection_Item] c on p.wineryId = c.WineryId where p.Id=@productId and c.id=@CollectionId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@productId", productId));
            parameterList.Add(GetParameter("@CollectionId", CollectionId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ret = true;
                    }
                }
            }

            return ret;
        }

        public List<Department> GetDepartments(int MemberId)
        {
            var departmentList = new List<Department>();

            string sql = "select [Id],DepartmentTitle,DepartmentCode from Departments where MemberId = @MemberId";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", MemberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.Text))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        departmentList.Add(new Department
                        {
                            department_code = Convert.ToString(dataReader["DepartmentCode"]),
                            id = Convert.ToInt32(dataReader["Id"]),
                            department_title = Convert.ToString(dataReader["DepartmentTitle"]),
                            member_id = MemberId
                        });
                    }
                }
            }

            return departmentList;
        }

        public List<ProductCollection> GetCollectionListByMember(int memberId)
        {
            var collectionList = new List<ProductCollection>();

            string sql = "GetCollectionList";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", memberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        collectionList.Add(new ProductCollection
                        {
                            collection_name = Convert.ToString(dataReader["CollectionName"]),
                            id = Convert.ToInt32(dataReader["Id"]),
                            sort_order = Convert.ToInt32(dataReader["SortOrder"]),
                            member_id = memberId
                        });
                    }
                }
            }

            return collectionList;
        }

        public ProductCollection AddUpdateCollection(ProductCollection request)
        {

            string sql = "AddUpdateCollection";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", request.id));
            parameterList.Add(GetParameter("@WineryId", request.member_id));
            parameterList.Add(GetParameter("@CollectionName", request.collection_name));
            parameterList.Add(GetParameter("@SortOrder", request.sort_order));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        request.id = Convert.ToInt32(dataReader["CollectionId"]);
                    }
                }
            }

            return request;
        }

        public List<POSKey> GetPOSKeyListByMember(int memberId)
        {
            var collectionList = new List<POSKey>();

            string sql = "GetPOSKeys";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@MemberId", memberId));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var posKey = new POSKey
                        {
                            product_id = Convert.ToInt32(dataReader["ProductId"]),
                            id = Convert.ToInt32(dataReader["Id"]),
                            key_num = Convert.ToInt32(dataReader["KeyNum"]),
                            member_id = memberId,
                            date_added = Convert.ToDateTime(dataReader["DateAdded"])

                        };

                        try
                        {
                            if (dataReader["DateModified"] != DBNull.Value)
                            {
                                posKey.date_modified = Convert.ToDateTime(dataReader["DateModified"]);
                            }
                        }
                        catch { }
                        collectionList.Add(posKey);
                    }
                }
            }

            return collectionList;
        }

        public POSKeyOut AddUpdatePOSKey(POSKey request)
        {

            string sql = "AddUpdatePOSKey";
            POSKeyOut retObject = new POSKeyOut
            {
                key_num = request.key_num,
                member_id = request.member_id,
                product_id = request.product_id
            };

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", request.id));
            parameterList.Add(GetParameter("@WineryId", request.member_id));
            parameterList.Add(GetParameter("@ProductId", request.product_id));
            parameterList.Add(GetParameter("@KeyNum", request.key_num));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        retObject.id = Convert.ToInt32(dataReader["POSKeyId"]);
                    }
                }
            }

            return retObject;
        }

        public bool RemovePOSKey(int Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            string sqlQuery = "DELETE FROM [dbo].[WineryPOSKey] WHERE Id = @Id";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateItemImageURL(int itemId, string item_image)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ItemId", itemId));
            parameterList.Add(GetParameter("@ImageName", item_image));

            string sqlQuery = "Update Product set ImageURL=@ImageName WHERE Id = @ItemId";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool RemoveCollection(int Id, int MemberId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));
            parameterList.Add(GetParameter("@MemberId", MemberId));

            string sqlQuery = "delete from [Collection_Item] where id=@Id and [WineryId] = @MemberId";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool RemoveItem(int Id)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));

            string sqlQuery = "delete from [product] where id=@Id";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool InsertProductvariant(int ProductId, int variantId)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@ProductId", ProductId));
            parameterList.Add(GetParameter("@variantId", variantId));

            string sqlQuery = "INSERT INTO [dbo].[Product_variant] ([ProductId],[variantId]) VALUES (@ProductId,@variantId)";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public bool UpdateItemToCollection(int Id, string Collections)
        {
            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", Id));
            parameterList.Add(GetParameter("@Collections", Collections));

            string sqlQuery = "update product set Collections = @Collections where id=@Id";

            return ExecuteNonQuery(sqlQuery, parameterList, CommandType.Text) > 0;
        }

        public List<ProductDetails> GetCollectionProductsById(int collection_id)
        {
            var lstProducts = new List<ProductDetails>();

            string sql = "GetCollectionProductsById";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@CollectionId", collection_id));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        var model = new ProductDetails();
                        model.id = Convert.ToInt32(dataReader["Id"]); ;
                        if (Convert.ToString(dataReader["ImageURL"]).Length > 0)
                            model.item_image = string.Format("{0}{1}/{2}", "https://cdncellarpass.blob.core.windows.net", "/photos/product_images", Convert.ToString(dataReader["ImageURL"]));
                        else
                            model.item_image = "";

                        int status = Convert.ToInt16(dataReader["Status"]);

                        if (status == 1 || status == 2)
                            model.is_active = true;
                        else
                            model.is_active = false;

                        if (status == 1)
                            model.is_public = true;
                        else
                            model.is_public = false;

                        model.item_name = Convert.ToString(dataReader["ProductName"]);
                        model.item_sku = Convert.ToString(dataReader["Sku"]);
                        model.retail_price = Convert.ToDecimal(dataReader["RetailPrice"]);

                        model.charge_tax = Convert.ToBoolean(dataReader["ChargeSalesTax"]);
                        model.club_price = Convert.ToDecimal(dataReader["ClubPrice"]);

                        model.cost = Convert.ToDecimal(dataReader["Cost"]);
                        model.custom_field_1 = Convert.ToString(dataReader["CustomField1"]);
                        model.custom_field_2 = Convert.ToString(dataReader["CustomField2"]);
                        model.display_in_pos = Convert.ToBoolean(dataReader["DisplayinPOS"]);
                        model.fulfillment_service = Convert.ToString(dataReader["FulfillmentService"]);
                        model.inventory_mode = Convert.ToInt32(dataReader["InventoryPolicy"]);
                        model.in_stock_quantity = Convert.ToInt32(dataReader["InventoryQuantity"]);
                        model.item_category = Convert.ToInt32(dataReader["WineCategory"]);
                        model.item_desc = Convert.ToString(dataReader["Description"]);

                        ItemDimensions itemDimensions = new ItemDimensions();

                        itemDimensions.height = Convert.ToDecimal(dataReader["Height"]);
                        itemDimensions.length = Convert.ToDecimal(dataReader["Length"]);
                        itemDimensions.width = Convert.ToDecimal(dataReader["Width"]);

                        model.item_dimensions = itemDimensions;

                        model.item_weight = Convert.ToDecimal(dataReader["ItemWeight"]);
                        model.requires_shipping = Convert.ToBoolean(dataReader["RequiresShipping"]);
                        model.sale_price = Convert.ToDecimal(dataReader["SalePrice"]);
                        //model.exclude_from_discounts = Convert.ToBoolean(dataReader["ExcludefromDiscounts"]);

                        string tags = Convert.ToString(dataReader["Tags"]);

                        model.tags = tags.Split(',').ToList();

                        string collections = Convert.ToString(dataReader["Collections"]);

                        if (collections.Length > 0)
                            model.collections = GetCollectionList(collections);

                        model.unit_type = Convert.ToInt32(dataReader["UnitType"]);
                        model.volume = Convert.ToString(dataReader["Volume"]);
                        model.weight_option = Convert.ToInt32(dataReader["Weightoption"]);

                        model.item_variants = GetProductVariantsByProductId(model.id);

                        lstProducts.Add(model);
                    }
                }
            }
            return lstProducts;
        }

        public int AddUpdateItem(AddUpdateItemRequest request)
        {
            int ProductId = 0;
            string sql = "AddUpdateItem";

            var parameterList = new List<DbParameter>();
            parameterList.Add(GetParameter("@Id", request.id));
            parameterList.Add(GetParameter("@WineryId", request.member_id));
            parameterList.Add(GetParameter("@SKU", request.item_sku));
            parameterList.Add(GetParameter("@ProductName", request.item_name));
            parameterList.Add(GetParameter("@RetailPrice", request.retail_price));

            if (request.status == 1 || request.status == 2)
                parameterList.Add(GetParameter("@Active", true));
            else
                parameterList.Add(GetParameter("@Active", false));

            parameterList.Add(GetParameter("@Description", request.item_desc));
            parameterList.Add(GetParameter("@WineCategory", request.item_category));
            parameterList.Add(GetParameter("@CustomField1", request.custom_field_1));
            parameterList.Add(GetParameter("@CustomField2", request.custom_field_2));
            parameterList.Add(GetParameter("@UnitType", request.unit_type));
            //parameterList.Add(GetParameter("@ImageURL", request.item_image));
            parameterList.Add(GetParameter("@CurrentUser", request.current_user));
            parameterList.Add(GetParameter("@DisplayinPOS", request.display_in_pos));
            parameterList.Add(GetParameter("@ChargeSalesTax", request.charge_tax));
            parameterList.Add(GetParameter("@Status", request.status));
            parameterList.Add(GetParameter("@SalePrice", request.sale_price));
            parameterList.Add(GetParameter("@Collections", string.Join(',', request.collections)));
            parameterList.Add(GetParameter("@Tags", string.Join(',', request.tags)));
            parameterList.Add(GetParameter("@ItemWeight", request.item_weight));
            parameterList.Add(GetParameter("@Weightoption", request.weight_option));
            parameterList.Add(GetParameter("@Length", request.item_dimensions.length));
            parameterList.Add(GetParameter("@Width", request.item_dimensions.width));
            parameterList.Add(GetParameter("@Height", request.item_dimensions.height));
            parameterList.Add(GetParameter("@RequiresShipping", request.requires_shipping));
            parameterList.Add(GetParameter("@FulfillmentService", request.fulfillment_service));
            parameterList.Add(GetParameter("@InventoryPolicy", request.inventory_mode));
            parameterList.Add(GetParameter("@InventoryQuantity", request.in_stock_quantity));
            parameterList.Add(GetParameter("@Volume", request.volume));
            parameterList.Add(GetParameter("@Cost", request.cost));
            parameterList.Add(GetParameter("@ClubPrice", request.club_price));
            parameterList.Add(GetParameter("@ExcludefromDiscounts", request.exclude_from_discounts));
            parameterList.Add(GetParameter("@ExternalURL", request.external_url));
            parameterList.Add(GetParameter("@DepartmentId", request.department_id));
            parameterList.Add(GetParameter("@VendorId", request.vendor_id));
            parameterList.Add(GetParameter("@ProductTemplate", request.product_template));
            parameterList.Add(GetParameter("@MetaTaxType", request.meta_tax_type));
            parameterList.Add(GetParameter("@MetaItemType", request.meta_item_type));
            parameterList.Add(GetParameter("@MetaBrandKey", request.meta_brand_key));
            parameterList.Add(GetParameter("@MetaProductKey", request.meta_product_key));
            parameterList.Add(GetParameter("@AccessSecurityId", request.access_and_security));
            parameterList.Add(GetParameter("@AccessSecurityTags", request.access_security_tags_Ids));
            parameterList.Add(GetParameter("@Clubs", request.access_security_clubs_Ids));

            using (DbDataReader dataReader = GetDataReader(sql, parameterList, CommandType.StoredProcedure))
            {
                if (dataReader != null && dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        ProductId = Convert.ToInt32(dataReader["ProductId"]);
                    }
                }
            }

            return ProductId;
        }

    }
}
