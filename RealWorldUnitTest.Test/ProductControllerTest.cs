using Microsoft.AspNetCore.Mvc;
using Moq;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Models;
using RealWorldUnitTest.Web.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RealWorldUnitTest.Test
{
    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsController _controller;
        private List<Product> _products;

        public ProductControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsController(_mockRepo.Object);
            _products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Kalem",
                    Price = 100,
                    Stok = 12,
                    Color = "Kırmızı"
                },
                new Product
                {
                    Id = 2,
                    Name = "Defter",
                    Price = 200,
                    Stok = 10,
                    Color = "Mavi"
                }
            };
        }

        [Fact]
        public async Task Index_ActionExecute_ReturnView()
        {
            var result = await _controller.Index();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void Index_ActionExecute_ReturnProductList()
        {

            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(_products);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal<int>(2, productList.Count());
        }

        [Fact]
        public async void Detail_IdIsNul_ReturnRedirectToIndex()
        {
            var result = await _controller.Details(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void Detail_IdInValid_ReturnNotFound()
        {
            Product p = null;
            _mockRepo.Setup(repo => repo.GetById(0)).ReturnsAsync(p);

            var result = await _controller.Details(0);
            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(2)]
        public async void Detail_IdValid_ReturnProduct(int productId)
        {
            var product = _products.FirstOrDefault(v => v.Id == productId);
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Details(productId);
            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        #region Create

        [Fact]
        public void Create_Action_ReturnViewResult()
        {
            var result = _controller.Create();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void CreatePost_InValidModelState_ReturnView()
        {
            _controller.ModelState.AddModelError("Name", "Name Alanı boştur");
            var result = await _controller.Create(_products.First());

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async void CreatePost_InValidModelState_NeverCreateExecute()
        {
            _controller.ModelState.AddModelError("Name", "Name Alanı boştur");
            var result = await _controller.Create(_products.First());

            _mockRepo.Verify(r => r.Create(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async void CreatePost_ValidModelState_ReturnRedirectToIndex()
        {
            //Bu test create için değil redirect testi.o yüzden mock yapmadık
            var result = await _controller.Create(_products.First());
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePost_ValidModelState_CreateMethodExecute()
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.Create(It.IsAny<Product>())).Callback<Product>(x => product = x);

            var result = await _controller.Create(_products.First());

            //Create in çalışıp çalışmadığını kontrol eder.
            _mockRepo.Verify(r => r.Create(It.IsAny<Product>()), Times.Once);

            Assert.Equal(_products.First().Id, product.Id);
        }

        #endregion

        #region Edit

        [Fact]
        public async void Edit_IdIsNull_ReturnToIndex()
        {
            var result = await _controller.Edit(null);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(3)]
        public async void Edit_InvalidId_ReturnNotFound(int pId)
        {
            //Product product = null;
            _mockRepo.Setup(re => re.GetById(pId)).ReturnsAsync(_products.FirstOrDefault(v=> v.Id == pId));

            var result = await _controller.Edit(pId);
            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(2)]
        public async void Edit_ActionExecuted_ReturnView(int pId)
        {
            var product = _products.FirstOrDefault(v => v.Id == pId);
            _mockRepo.Setup(re => re.GetById(pId)).ReturnsAsync(product);

            var result = await _controller.Edit(pId);
            var viewResult = Assert.IsType<ViewResult>(result);

            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product.Id, resultProduct.Id);
        }

        [Theory]
        [InlineData(1)]
        public void EditPost_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = _controller.Edit(2, _products.FirstOrDefault(v => v.Id == productId));
            var redirect = Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void EditPost_InValidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name", "Name Boş");

            var result = _controller.Edit(productId, _products.FirstOrDefault(v => v.Id == productId));
            var redirect = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(redirect.Model);
        }


        [Theory]
        [InlineData(1)]
        public void EditPost_ValidModelState_ReturnIndexView(int productId)
        {
            var result = _controller.Edit(productId, _products.FirstOrDefault(v => v.Id == productId));
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }


        [Theory]
        [InlineData(1)]
        public void EditPost_ValidModelState_UpdateMethodExecute(int productId)
        {
            var product = _products.FirstOrDefault(v => v.Id == productId);
            _mockRepo.Setup(re => re.Update(product));

            var result = _controller.Edit(productId, product);
            _mockRepo.Verify(re => re.Update(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public async void Delete_IdISNull_NotFound()
        {
            var result = await _controller.Delete(null);
            var redirect = Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(0)]
        public async void Delete_IdISNotEqualProduct_NotFound(int id)
        {
            Product product = null;

            _mockRepo.Setup(re => re.GetById(id)).ReturnsAsync(product);
            var result = await _controller.Delete(id);

            var redirect = Assert.IsType<NotFoundResult>(result);
        }


        [Theory]
        [InlineData(1)]
        public async void Delete_ActionExecutes_ReturnProduct(int id)
        {
            var product = _products.FirstOrDefault(v => v.Id == id);
            _mockRepo.Setup(v => v.GetById(id)).ReturnsAsync(product);

            var result = await _controller.Delete(id);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsAssignableFrom<Product>(viewResult.Model);
        }


        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_ReturnRedirectToIndexAction(int id)
        {
            var result = await _controller.DeleteConfirmed(id);
            Assert.IsType<RedirectToActionResult>(result);
        }


        [Theory]
        [InlineData(1)]
        public async void DeleteConfirmed_ActionExecutes_DeleteMethodExecute(int id)
        {
            var product = _products.FirstOrDefault(v => v.Id == id);
            _mockRepo.Setup(v => v.Delete(product));

            var result = await _controller.DeleteConfirmed(id);
            _mockRepo.Verify(re => re.Delete(It.IsAny<Product>()), Times.Once);
        }

        #endregion
    }
}
