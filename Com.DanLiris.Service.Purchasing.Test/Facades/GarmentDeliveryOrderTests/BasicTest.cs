﻿using Com.DanLiris.Service.Purchasing.Lib;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentDeliveryOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentExternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentInternalPurchaseOrderFacades;
using Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentPurchaseRequestFacades;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentExternalPurchaseOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentDeliveryOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentExternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentInternalPurchaseOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.GarmentPurchaseRequestDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Facades.GarmentDeliveryOrderTests
{
    public class BasicTest
    {
        private const string ENTITY = "GarmentDeliveryOrder";

        private const string USERNAME = "Unit Test";
        private IServiceProvider ServiceProvider { get; set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string GetCurrentMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);

            return string.Concat(sf.GetMethod().Name, "_", ENTITY);
        }

        private PurchasingDbContext _dbContext(string testName)
        {
            DbContextOptionsBuilder<PurchasingDbContext> optionsBuilder = new DbContextOptionsBuilder<PurchasingDbContext>();
            optionsBuilder
                .UseInMemoryDatabase(testName)
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            PurchasingDbContext dbContext = new PurchasingDbContext(optionsBuilder.Options);

            return dbContext;
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            HttpResponseMessage message = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            message.Content = new StringContent("{\"apiVersion\":\"1.0\",\"statusCode\":200,\"message\":\"Ok\",\"data\":[{\"Id\":7,\"code\":\"USD\",\"rate\":13700.0,\"date\":\"2018/10/20\"}],\"info\":{\"count\":1,\"page\":1,\"size\":1,\"total\":2,\"order\":{\"date\":\"desc\"},\"select\":[\"Id\",\"code\",\"rate\",\"date\"]}}");
            var HttpClientService = new Mock<IHttpClientService>();
            HttpClientService
                .Setup(x => x.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(message);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(HttpClientService.Object);

            return serviceProvider;
        }

        private GarmentDeliveryOrderDataUtil dataUtil(GarmentDeliveryOrderFacade facade, string testName)
        {
            var garmentPurchaseRequestFacade = new GarmentPurchaseRequestFacade(_dbContext(testName));
            var garmentPurchaseRequestDataUtil = new GarmentPurchaseRequestDataUtil(garmentPurchaseRequestFacade);

            var garmentInternalPurchaseOrderFacade = new GarmentInternalPurchaseOrderFacade(_dbContext(testName));
            var garmentInternalPurchaseOrderDataUtil = new GarmentInternalPurchaseOrderDataUtil(garmentInternalPurchaseOrderFacade, garmentPurchaseRequestDataUtil);

            var garmentExternalPurchaseOrderFacade = new GarmentExternalPurchaseOrderFacade(ServiceProvider,_dbContext(testName));
            var garmentExternalPurchaseOrderDataUtil = new GarmentExternalPurchaseOrderDataUtil(garmentExternalPurchaseOrderFacade, garmentInternalPurchaseOrderDataUtil);

            return new GarmentDeliveryOrderDataUtil(facade, garmentExternalPurchaseOrderDataUtil);
        }

        [Fact]
        public async void Should_Success_Create_Data()
        {

            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            var Response = await facade.Create(model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Create_Data_2()
        {

            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData2();
            var Response = await facade.Create(model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Create_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = dataUtil(facade, GetCurrentMethod()).GetNewData();
            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Create(null, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Update_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                Id = model.Id,
                supplier = new SupplierViewModel(),
                customsId = 1,
                billNo = "test",
                paymentBill = "test",
                totalAmount = 1,
                shipmentType = "test",
                shipmentNo = "test",
                paymentMethod = "test",
                paymentType = "test",
                docurrency = new CurrencyViewModel(),
                items = new List<GarmentDeliveryOrderItemViewModel>
                {
                    new GarmentDeliveryOrderItemViewModel
                    {
                        Id = model.Items.ElementAt(0).Id,
                        purchaseOrderExternal = new PurchaseOrderExternal{ Id = 1,no="test"},
                        paymentDueDays = 1,
                        currency = new CurrencyViewModel(),

                        fulfillments = new List<GarmentDeliveryOrderFulfillmentViewModel>
                        {
                            new GarmentDeliveryOrderFulfillmentViewModel
                            {
                                Id = model.Items.ElementAt(0).Details.ElementAt(0).Id,
                                pOId = 1,
                                pOItemId = 1,
                                conversion = 0,
                                quantityCorrection = 0,
                                pricePerDealUnit = 0,
                                priceTotalCorrection = 0,
                                isSave = true
                            }
                        }
                    }
                }

            };

            var Response = await facade.Update((int)model.Id, viewModel, model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Update_Data2()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData2();

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                Id = model.Id,
                supplier = new SupplierViewModel(),
                customsId = 1,
                billNo = "test",
                paymentBill = "test",
                totalAmount = 1,
                shipmentType = "test",
                shipmentNo = "test",
                paymentMethod = "test",
                paymentType = "test",
                docurrency = new CurrencyViewModel(),
                items = new List<GarmentDeliveryOrderItemViewModel>
                {
                    new GarmentDeliveryOrderItemViewModel
                    {
                        Id = model.Items.ElementAt(0).Id,
                        purchaseOrderExternal = new PurchaseOrderExternal{ Id = 1,no="test"},
                        paymentDueDays = 1,
                        currency = new CurrencyViewModel(),

                        fulfillments = new List<GarmentDeliveryOrderFulfillmentViewModel>
                        {
                            new GarmentDeliveryOrderFulfillmentViewModel
                            {
                                Id = model.Items.ElementAt(0).Details.ElementAt(0).Id,
                                pOId = 1,
                                pOItemId = 1,
                                conversion = 0,
                                quantityCorrection = 0,
                                pricePerDealUnit = 0,
                                priceTotalCorrection = 0,
                                isSave = false
                            }
                        }
                    }
                }

            };

            var Response = await facade.Update((int)model.Id, viewModel, model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Update_Data3()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData3();

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                Id = model.Id,
                supplier = new SupplierViewModel(),
                customsId = 1,
                billNo = "test",
                paymentBill = "test",
                totalAmount = 1,
                shipmentType = "test",
                shipmentNo = "test",
                paymentMethod = "test",
                paymentType = "test",
                docurrency = new CurrencyViewModel(),
                items = new List<GarmentDeliveryOrderItemViewModel>
                {
                    new GarmentDeliveryOrderItemViewModel
                    {
                        Id = model.Items.ElementAt(0).Id,
                        purchaseOrderExternal = new PurchaseOrderExternal{ Id = 1,no="test"},
                        paymentDueDays = 1,
                        currency = new CurrencyViewModel(),

                        fulfillments = new List<GarmentDeliveryOrderFulfillmentViewModel>
                        {
                            new GarmentDeliveryOrderFulfillmentViewModel
                            {
                                Id = model.Items.ElementAt(0).Details.ElementAt(0).Id,
                                pOId = 1,
                                pOItemId = 1,
                                conversion = 0,
                                quantityCorrection = 0,
                                pricePerDealUnit = 0,
                                priceTotalCorrection = 0,
                                isSave = true
                            }
                        }
                    }
                }

            };

            var Response = await facade.Update((int)model.Id, viewModel, model, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Update_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            Exception errorInvalidId = await Assert.ThrowsAsync<Exception>(async () => await facade.Update(0, null, model, USERNAME));
            Assert.NotNull(errorInvalidId.Message);
        }

        [Fact]
        public async void Should_Success_Delete_Data()
        {
            var facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = await facade.Delete((int)model.Id, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Delete_Data2()
        {
            var facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData2();
            var Response = await facade.Delete((int)model.Id, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Success_Delete_Data3()
        {
            var facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData3();
            var Response = await facade.Delete((int)model.Id, USERNAME);
            Assert.NotEqual(Response, 0);
        }

        [Fact]
        public async void Should_Error_Delete_Data()
        {
            var facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));

            Exception e = await Assert.ThrowsAsync<Exception>(async () => await facade.Delete(0, USERNAME));
            Assert.NotNull(e.Message);
        }

        [Fact]
        public async void Should_Success_Get_All_Data()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.Read();
            Assert.NotEqual(Response.Item1.Count, 0);
        }

        [Fact]
        public async void Should_Success_Get_Data_By_Id()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
            var Response = facade.ReadById((int)model.Id);
            Assert.NotNull(Response);
        }
		[Fact]
		public async void Should_Success_Get_Data_By_Supplier()
		{
			GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
			var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
			var Response = facade.ReadBySupplier("code","{}");
			Assert.NotNull(Response);
		}
		[Fact]
		public async void Should_Success_Get_Data_For_Customs()
		{
			GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
			var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
			var Response = facade.DOForCustoms("code", "{}");
			Assert.NotNull(Response);
		}
		[Fact]
		public async void Should_Success_Get_Data_Is_Received()
		{
			GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
			var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
			model.IsInvoice = true;
			List<int> _id = new List<int>();
			_id.Add((int)model.Id);
			var Response = facade.IsReceived(_id);
			Assert.NotNull(Response);
		}
		[Fact]
		public async void Should_Success_Get_Data_Is_Received2()
		{
			GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
			var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();
			model.IsInvoice = false;
			foreach(var data in model.Items)
			{
				foreach(var item in data.Details)
				{
					item.ReceiptQuantity = 2;
				}
			}
			List<int> _id = new List<int>();
			_id.Add((int)model.Id);
			var Response = facade.IsReceived(_id);
			Assert.NotNull(Response);
		}
		[Fact]
        public void Should_Success_Validate_Data()
        {
            GarmentDeliveryOrderViewModel nullViewModel = new GarmentDeliveryOrderViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                supplier = new SupplierViewModel(),

            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Data_Where_DoDate_GreaterThan_ArrivalDate()
        {
            GarmentDeliveryOrderViewModel nullViewModel = new GarmentDeliveryOrderViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                doDate = new DateTimeOffset(DateTime.Today).AddDays(7),
                arrivalDate = new DateTimeOffset(DateTime.Today).AddDays(1),
                supplier = new SupplierViewModel(),
            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Data_Item()
        {
            GarmentDeliveryOrderViewModel nullViewModel = new GarmentDeliveryOrderViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                supplier = new SupplierViewModel(),
                customsId = 1,
                billNo = "test",
                paymentBill = "test",
                totalAmount = 1,
                shipmentType = "test",
                shipmentNo = "test",
                paymentMethod = "test",
                paymentType = "test",
                docurrency = new CurrencyViewModel(),
                items = new List<GarmentDeliveryOrderItemViewModel>
                {
                    new GarmentDeliveryOrderItemViewModel
                    {
                        purchaseOrderExternal = null,
                        paymentDueDays = 1,
                        currency = new CurrencyViewModel(),
                        
                        fulfillments = new List<GarmentDeliveryOrderFulfillmentViewModel>
                        {
                            new GarmentDeliveryOrderFulfillmentViewModel
                            {
                                pOId = 1,
                                pOItemId = 1,
                                conversion = 1
                            }
                        }
                    }
                }

            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Data_Fulfillment_Null()
        {
            GarmentDeliveryOrderViewModel nullViewModel = new GarmentDeliveryOrderViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                supplier = new SupplierViewModel(),
                customsId = 1,
                billNo = "test",
                paymentBill = "test",
                totalAmount = 1,
                shipmentType = "test",
                shipmentNo = "test",
                paymentMethod = "test",
                paymentType = "test",
                docurrency = new CurrencyViewModel(),
                items = new List<GarmentDeliveryOrderItemViewModel>
                {
                    new GarmentDeliveryOrderItemViewModel
                    {
                        purchaseOrderExternal = new PurchaseOrderExternal{ Id = 1,no="test"},
                        paymentDueDays = 1,
                        currency = new CurrencyViewModel(),
                        fulfillments = null
                    }
                }

            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public void Should_Success_Validate_Data_Fulfillment_With_Conversion_0()
        {
            GarmentDeliveryOrderViewModel nullViewModel = new GarmentDeliveryOrderViewModel();
            Assert.True(nullViewModel.Validate(null).Count() > 0);

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                supplier = new SupplierViewModel(),
                customsId = 1,
                billNo = "test",
                paymentBill = "test",
                totalAmount = 1,
                shipmentType = "test",
                shipmentNo = "test",
                paymentMethod = "test",
                paymentType = "test",
                docurrency = new CurrencyViewModel(),
                items = new List<GarmentDeliveryOrderItemViewModel>
                {
                    new GarmentDeliveryOrderItemViewModel
                    {
                        purchaseOrderExternal = new PurchaseOrderExternal{ Id = 1,no="test"},
                        paymentDueDays = 1,
                        currency = new CurrencyViewModel(),

                        fulfillments = new List<GarmentDeliveryOrderFulfillmentViewModel>
                        {
                            new GarmentDeliveryOrderFulfillmentViewModel
                            {
                                pOId = 1,
                                pOItemId = 1,
                                conversion = 0,
                                quantityCorrection = 0,
                                pricePerDealUnit = 0,
                                priceTotalCorrection = 0,
                            }
                        }
                    }
                }

            };
            Assert.True(viewModel.Validate(null).Count() > 0);
        }

        [Fact]
        public async void Should_Success_Validate_Data_Duplicate()
        {
            GarmentDeliveryOrderFacade facade = new GarmentDeliveryOrderFacade(GetServiceProvider().Object, _dbContext(GetCurrentMethod()));
            var model = await dataUtil(facade, GetCurrentMethod()).GetTestData();

            GarmentDeliveryOrderViewModel viewModel = new GarmentDeliveryOrderViewModel
            {
                supplier = new SupplierViewModel(),
                incomeTax = new IncomeTaxViewModel(),
                docurrency = new CurrencyViewModel(),
            };
            viewModel.Id = model.Id + 1;
            viewModel.doNo = model.DONo;
            viewModel.supplier.Id = model.SupplierId;
            viewModel.doDate = model.DODate;
            viewModel.arrivalDate = model.ArrivalDate;
            viewModel.docurrency.Id = (long)model.DOCurrencyId;
            viewModel.docurrency.Code = model.DOCurrencyCode;
            viewModel.incomeTax.Id = (int)model.IncomeTaxId;
            viewModel.incomeTax.Name = model.IncomeTaxName;
            viewModel.incomeTax.Rate = (double)model.IncomeTaxRate;
            viewModel.remark = model.Remark;
            viewModel.isCorrection = (bool)model.IsCorrection;
            viewModel.useVat = (bool)model.UseVat;
            viewModel.useIncomeTax = (bool)model.UseIncomeTax;
            

            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.
                Setup(x => x.GetService(typeof(PurchasingDbContext)))
                .Returns(_dbContext(GetCurrentMethod()));

            ValidationContext validationContext = new ValidationContext(viewModel, serviceProvider.Object, null);

            var validationResultCreate = viewModel.Validate(validationContext).ToList();

            var errorDuplicateDONo = validationResultCreate.SingleOrDefault(r => r.ErrorMessage.Equals("DoNo is already exist"));
            Assert.NotNull(errorDuplicateDONo);
        }
    }
}
