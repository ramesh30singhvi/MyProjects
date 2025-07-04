using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using RestSharp;
using Newtonsoft.Json;
using CPReservationApi.WebApi.ViewModels;
using CPReservationApi.DAL;

namespace CPReservationApi.WebApi.Services
{
    public class BigCommerceClient
    {
        public const string BASE_URL = "https://api.bigcommerce.com/stores/";
        public const string CUSTOMER_API_V3 = "/v3/customers";
        public const string ORDER_API_V2 = "/v2/orders";
        public const string CART_API_V3 = "/v3/carts";
        public const string CART_REDIRECT_API_V3 = "/v3/carts/{cartId}/redirect_urls";
        public const string products_api_v3 = "/v3/catalog/products";
        public const string products_getById_api_v3 = "/v3/catalog/products/{product_id}";

        private string _StoreId = "";
        private string _AcessToken = "";
        private string _DefaultQueryLimit = "250";
        public BigCommerceClient(string storeId, string accessToken, string defaultQueryLimit = "250")
        {
            _StoreId = storeId;
            _AcessToken = accessToken;
            if (!string.IsNullOrWhiteSpace(defaultQueryLimit))
                _DefaultQueryLimit = defaultQueryLimit;
        }

        public IRestResponse CallApi(string path, Method method, List<KeyValuePair<String, String>> queryParams = null, Dictionary<String, String> pathParams = null, Object postBody = null)
        {
            // Create Request
            string requestPath = BASE_URL + this._StoreId + path;
            var client = new RestClient(requestPath);
            var request = new RestRequest(method);
            request.AddHeader("accept", "application/json");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("x-auth-token", this._AcessToken);

            if (pathParams == null) pathParams = new Dictionary<string, string>();
            if (queryParams == null) queryParams = new List<KeyValuePair<string, string>>();

            // add path parameter, if any
            foreach (var param in pathParams)
                request.AddParameter(param.Key, param.Value, ParameterType.UrlSegment);

            // add query parameter, if any
            foreach (var param in queryParams)
                request.AddQueryParameter(param.Key, param.Value);

            if (postBody != null) // http body (model or byte[]) parameter
            {
                request.AddParameter("application/json", postBody, ParameterType.RequestBody);
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            LogDAL log = new LogDAL(Common.Common.ConnectionString);
            string logData = "URL: " + requestPath;
            if (queryParams != null && queryParams.Count > 0)
            {
                logData += ", Request:" + JsonConvert.SerializeObject(queryParams);
            }
            else if (postBody != null)
            {
                logData += ", Request:" + (string)postBody;
            }
            log.InsertLog("Webapi:BigCommerceClient", logData, "CoreApi User", 3);
            IRestResponse response = client.Execute(request);
            if (response != null && response.Content != null)
            {
                log.InsertLog("Webapi:BigCommerceClient", "URL: " + requestPath + ", response:" + response.Content, "CoreApi User", 3);
            }
            return response;
        }

        public ApiResponse GetAllCustomers(GetAllCustomersParams getAllCustomersParams)
        {
            ApiResponse _Customers = new ApiResponse();
            var localVarPath = CUSTOMER_API_V3;
            var localVarQueryParams = new List<KeyValuePair<String, String>>();

            localVarQueryParams.Add(new KeyValuePair<string, string>("limit", _DefaultQueryLimit));
            if (getAllCustomersParams.page != null && getAllCustomersParams.page > 0)
                localVarQueryParams.Add(new KeyValuePair<string, string>("page", getAllCustomersParams.page.ToString()));

            IRestResponse response = CallApi(path: localVarPath, method: Method.GET, queryParams: localVarQueryParams);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                //--Conver Json-Response to ViewModel
                _Customers = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                _Customers.status = 1;
                _Customers.response = new
                {
                    success = "Success"
                };

            }
            else
            {
                //--Create Failuer-Response
                _Customers.status = -101;
                _Customers.response = new
                {
                    error = JsonConvert.DeserializeObject(response.Content)
                };
                _Customers.data = new List<string>();
                _Customers.meta = new List<string>();
            }

            return _Customers;
        }

        public ApiResponse GetCustomerById(int id)
        {
            ApiResponse _Customers = new ApiResponse();

            var localVarPath = CUSTOMER_API_V3;
            var localVarQueryParams = new List<KeyValuePair<String, String>>();

            localVarQueryParams.Add(new KeyValuePair<string, string>("limit", _DefaultQueryLimit));
            localVarQueryParams.Add(new KeyValuePair<string, string>("id:in", id.ToString()));
            localVarQueryParams.Add(new KeyValuePair<string, string>("include", "addresses"));

            IRestResponse response = CallApi(path: localVarPath, method: Method.GET, queryParams: localVarQueryParams);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                //--Conver Json-Response to ViewModel
                _Customers = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                _Customers.status = 1;
                _Customers.response = new
                {
                    success = "Success"
                };

            }
            else
            {
                //--Create Failuer-Response
                _Customers.status = -101;
                _Customers.response = new
                {
                    error = JsonConvert.DeserializeObject(response.Content)
                };
                _Customers.data = new List<string>();
                _Customers.meta = new List<string>();
            }

            return _Customers;
        }

        public SearchCustomerViewModel SearchCustomerRecords(SearchCustomerParameters searchCustomerParameters)
        {
            SearchCustomerViewModel _Customers = new SearchCustomerViewModel();

            var localVarPath = CUSTOMER_API_V3;
            var localVarQueryParams = new List<KeyValuePair<String, String>>();

            // Add query parameters in request if any
            localVarQueryParams.Add(new KeyValuePair<string, string>("limit", _DefaultQueryLimit));
            localVarQueryParams.Add(new KeyValuePair<string, string>("include", "addresses"));
            // adding parameters according to request
            if (searchCustomerParameters.email != null)
                localVarQueryParams.Add(new KeyValuePair<string, string>("email:in", searchCustomerParameters.email));
            if (searchCustomerParameters.name != null)
                localVarQueryParams.Add(new KeyValuePair<string, string>("name:like", searchCustomerParameters.name));
            if (searchCustomerParameters.page != null && searchCustomerParameters.page != 0)
                localVarQueryParams.Add(new KeyValuePair<string, string>("page", searchCustomerParameters.page.ToString()));

            IRestResponse response = CallApi(path: localVarPath, method: Method.GET, queryParams: localVarQueryParams);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                //--Conver Json-Response to ViewModel
                _Customers = JsonConvert.DeserializeObject<SearchCustomerViewModel>(response.Content);
                _Customers.status = 1;
                _Customers.response = new
                {
                    success = "Success"
                };

            }
            else
            {
                //--Create Failuer-Response
                _Customers.status = -101;
                _Customers.response = new
                {
                    error = JsonConvert.DeserializeObject(response.Content)
                };
                _Customers.data = new List<SearchCustomersData_VM>();
                _Customers.meta = new List<string>();
            }

            return _Customers;
        }

        public ApiResponse CreateCustomers(List<CreateCustomers> _Param)
        {
            ApiResponse createCustomerResponse = new ApiResponse();

            var localVarPath = CUSTOMER_API_V3;
            Object localVarPostBody = null;

            if (_Param != null && _Param.Count > 0)
                localVarPostBody = JsonConvert.SerializeObject(_Param);

            IRestResponse response = CallApi(path: localVarPath, method: Method.POST, postBody: localVarPostBody);

            if (response.StatusCode == HttpStatusCode.OK)
            {

                //--Conver Json-Response to ViewModel
                createCustomerResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                createCustomerResponse.status = 1;
                createCustomerResponse.response = new
                {
                    success = "New Customers has been successfully created"
                };
            }
            else
            {
                object errorResponse = JsonConvert.DeserializeObject(response.Content);
                //--Create Failuer-Response
                createCustomerResponse.status = -101;
                createCustomerResponse.response = new
                {
                    error = errorResponse
                };
                createCustomerResponse.data = new List<String>();
                createCustomerResponse.meta = new List<String>();
            }
            return createCustomerResponse;
        }

        public ApiResponse CreateOrder(CreateOrderModel _Param)
        {
            //--Created Object of Order-Response-Class
            ApiResponse _Order = new ApiResponse();

            var localVarPath = ORDER_API_V2;
            Object localVarPostBody = null;

            if (_Param != null)
                localVarPostBody = JsonConvert.SerializeObject(_Param);

            IRestResponse response = CallApi(path: localVarPath, method: Method.POST, postBody: localVarPostBody);


            if (response.StatusCode == HttpStatusCode.Created)
            {

                //--Get Response from BigCommerce Server
                string _responseFromServer = response.Content;

                //--Conver Json-Response to ViewModel
                _Order.data = JsonConvert.DeserializeObject(_responseFromServer);
                _Order.status = 1;
                _Order.response = new
                {
                    success = "New Order has been successfully created"
                };
            }
            else
            {
                //--Create Failuer-Response
                object _error_response = JsonConvert.DeserializeObject(response.Content);

                //--Create Failuer-Response
                _Order.status = -101;
                _Order.response = new
                {
                    error = _error_response
                };
                _Order.data = null;
            }

            return _Order;
        }

        public ApiResponse GetAllOrders(OrdersListParams _Param)
        {
            ApiResponse orderViewModel = new ApiResponse();

            var localVarPath = ORDER_API_V2;
            var localVarQueryParams = new List<KeyValuePair<String, String>>();

            // Add query parameters in request if any
            localVarQueryParams.Add(new KeyValuePair<string, string>("limit", _DefaultQueryLimit));
            // Add query parameters in request if any
            if (_Param.page != null)
                localVarQueryParams.Add(new KeyValuePair<string, string>("page", _Param.page.ToString()));
            if (_Param.customer_id != null)
                localVarQueryParams.Add(new KeyValuePair<string, string>("customer_id", _Param.customer_id.ToString()));

            IRestResponse response = CallApi(path: localVarPath, method: Method.GET, queryParams: localVarQueryParams);

            if (response.Content == null || response.Content == "")
            {
                orderViewModel.data = new List<string>();
                orderViewModel.status = 1;
                orderViewModel.response = new
                {
                    success = "Success"
                };
                return orderViewModel;
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                //--Conver Json-Response to ViewModel
                orderViewModel.data = JsonConvert.DeserializeObject(response.Content);
                orderViewModel.status = 1;
                orderViewModel.response = new
                {
                    success = "Success"
                };

            }
            else
            {
                //--Create Failuer-Response
                orderViewModel.status = -101;
                orderViewModel.response = new
                {
                    error = JsonConvert.DeserializeObject(response.Content)
                };
                orderViewModel.data = new List<string>();
            }

            return orderViewModel;
        }

        public ApiResponse CreateCart(object _Param)
        {
            ApiResponse createCartResponse = new ApiResponse();

            var localVarPath = CART_API_V3;
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            Object localVarPostBody = null;

            if (_Param != null)
                localVarPostBody = JsonConvert.SerializeObject(_Param);

            localVarQueryParams.Add(new KeyValuePair<string, string>("include", "redirect_urls"));
            IRestResponse response = CallApi(path: localVarPath, method: Method.POST, postBody: localVarPostBody);


            if (response.StatusCode == HttpStatusCode.Created)
            {

                //--Conver Json-Response to ViewModel
                createCartResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                createCartResponse.status = 1;
                createCartResponse.response = new
                {
                    success = "New Cart has been successfully created"
                };
            }
            else
            {
                object errorResponse = JsonConvert.DeserializeObject(response.Content);
                //--Create Failuer-Response
                createCartResponse.status = -101;
                createCartResponse.response = new
                {
                    error = errorResponse
                };
                createCartResponse.data = null;
            }
            return createCartResponse;
        }

        public ApiResponse CreateCartRedirectUrls(string cartId)
        {
            ApiResponse createCartRedirectURLsRespone_VM = new ApiResponse();

            var localVarPath = CART_REDIRECT_API_V3;
            var localVarPathParams = new Dictionary<String, String>();

            if (cartId != null) localVarPathParams.Add("cartId", cartId); // path parameter

            IRestResponse response = CallApi(path: localVarPath, method: Method.POST, pathParams: localVarPathParams);

            if (response.StatusCode == HttpStatusCode.Created)
            {
                ApiResponse apiResponse = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                //--Conver Json-Response to ViewModel
                createCartRedirectURLsRespone_VM.data = apiResponse.data;
                createCartRedirectURLsRespone_VM.status = 1;
                createCartRedirectURLsRespone_VM.response = new
                {
                    success = "New Cart Redirect URLs has been successfully created"
                };
            }
            else
            {
                object errorResponse = JsonConvert.DeserializeObject(response.Content);
                //--Create Failuer-Response
                createCartRedirectURLsRespone_VM.status = -101;
                createCartRedirectURLsRespone_VM.response = new
                {
                    error = errorResponse
                };
                createCartRedirectURLsRespone_VM.data = null;
            }
            return createCartRedirectURLsRespone_VM;
        }

        public ProductList GetAllProducts(GetAllProductsParams getAllProductsParams)
        {
            ProductList _Products = new ProductList();

            var localVarPath = products_api_v3;
            var localVarQueryParams = new List<KeyValuePair<String, String>>();

            localVarQueryParams.Add(new KeyValuePair<string, string>("limit", _DefaultQueryLimit));
            localVarQueryParams.Add(new KeyValuePair<string, string>("include", "variants, options, images, primary_image"));
            if (getAllProductsParams.page != null && getAllProductsParams.page > 0)
                localVarQueryParams.Add(new KeyValuePair<string, string>("page", getAllProductsParams.page.ToString()));
            if (!String.IsNullOrEmpty(getAllProductsParams.name))
                localVarQueryParams.Add(new KeyValuePair<string, string>("name", getAllProductsParams.name));
            if (!String.IsNullOrEmpty(getAllProductsParams.keyword))
                localVarQueryParams.Add(new KeyValuePair<string, string>("keyword", getAllProductsParams.keyword));

            IRestResponse response = CallApi(path: localVarPath, method: Method.GET, queryParams: localVarQueryParams);


            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    _Products = JsonConvert.DeserializeObject<ProductList>(response.Content);
                }
            }
            catch
            {
                LogDAL logDAL = new LogDAL(Common.Common.ConnectionString);

                logDAL.InsertLog("BigCommerceGetAllProducts", "keyword: " + getAllProductsParams.keyword + ", Response: " + response.Content, "", 1, 0);
            }

            return _Products;
        }

        public ApiResponse GetProductById(int id)
        {
            ApiResponse _Product = new ApiResponse();

            var localVarPath = products_getById_api_v3;
            var localVarPathParams = new Dictionary<String, String>();

            localVarPathParams.Add("product_id", id.ToString()); // path parameter

            var localVarQueryParams = new List<KeyValuePair<String, String>>();

            localVarQueryParams.Add(new KeyValuePair<string, string>("limit", _DefaultQueryLimit));
            localVarQueryParams.Add(new KeyValuePair<string, string>("include", "variants, options, images, primary_image"));

            IRestResponse response = CallApi(path: localVarPath, method: Method.GET, queryParams: localVarQueryParams, pathParams: localVarPathParams);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                //--Conver Json-Response to ViewModel
                _Product = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                _Product.status = 1;
                _Product.response = new
                {
                    success = "Success"
                };

            }
            else
            {
                //--Create Failuer-Response
                _Product.status = -101;
                _Product.response = new
                {
                    error = JsonConvert.DeserializeObject(response.Content)
                };
                _Product.data = new List<string>();
                _Product.meta = new List<string>();
            }

            return _Product;
        }

        public ApiResponse CreateProduct(object _Param)
        {
            //--Created Object of Product-Response-Class
            ApiResponse _Product = new ApiResponse();

            var localVarPath = products_api_v3;
            Object localVarPostBody = null;

            if (_Param != null)
                localVarPostBody = JsonConvert.SerializeObject(_Param);

            IRestResponse response = CallApi(path: localVarPath, method: Method.POST, postBody: localVarPostBody);


            if (response.StatusCode == HttpStatusCode.OK)
            {

                //--Get Response from BigCommerce Server
                string _responseFromServer = response.Content;

                //--Conver Json-Response to ViewModel
                _Product = JsonConvert.DeserializeObject<ApiResponse>(_responseFromServer);
                _Product.status = 1;
                _Product.response = new
                {
                    success = "New Product has been successfully created"
                };
            }
            else
            {
                //--Create Failuer-Response
                object _error_response = JsonConvert.DeserializeObject(response.Content);

                //--Create Failuer-Response
                _Product.status = -101;
                _Product.response = new
                {
                    error = _error_response
                };
                _Product.data = null;
            }

            return _Product;
        }
    }
}
    