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
    public class ProductsApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;
        private readonly ProductsApiController _controller;
        private List<Product> _products;

        public ProductsApiControllerTest()
        {
            _mockRepo = new Mock<IRepository<Product>>();
            _controller = new ProductsApiController(_mockRepo.Object);
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
        public async void GetProducts_ActionExecute_ReturnOkResultWithProduct()
        {
            _mockRepo.Setup(c => c.GetAll()).ReturnsAsync(_products);
            var result = await _controller.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal(2, returnProducts.Count());
        }

        [Theory]
        [InlineData(0)]
        public async void GetProduct_IdInValid_ReturnNotFound(int id)
        {
            Product product = null;
            _mockRepo.Setup(re => re.GetById(id)).ReturnsAsync(product);

            var result = await _controller.GetProduct(id);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async void GetProduct_IdValid_ReturnOkResult(int id)
        {
            var product = _products.FirstOrDefault(v => v.Id == id);
            _mockRepo.Setup(v => v.GetById(id)).ReturnsAsync(product);

            var result = await _controller.GetProduct(id);
            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(id, returnProduct.Id);
        }


        [Theory]
        [InlineData(1)]
        public void PutProduct_IdIsNotEqual_ReturnBadRequestResult(int id)
        {
            var product = _products.FirstOrDefault(v => v.Id == id);

            var result = _controller.PutProduct(4, product);

            var returnResult = Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_ActionExecute_ReturnNoContentResult(int id)
        {
            var product = _products.FirstOrDefault(v => v.Id == id);
            _mockRepo.Setup(v => v.Update(product));
            var result = _controller.PutProduct(id, product);
            _mockRepo.Verify(v=> v.Update(product),Times.Once);
            Assert.IsType<NoContentResult>(result);
        }


        [Fact]
        public async void PostProduct_ActionExecute_CreatedActionResult()
        {
            var product = _products.First();
            _mockRepo.Setup(v => v.Create(product)).Returns(Task.CompletedTask);

            var result = await _controller.PostProduct(product);
            var createedActionResult = Assert.IsType<CreatedAtActionResult>(result);

            _mockRepo.Verify(v => v.Create(product), Times.Once);
            var getProduct = Assert.IsType<Product>(createedActionResult.Value);
            Assert.Equal("GetProduct", createedActionResult.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteProduct_IdInValid_ReturnNotFoundResult(int id)
        {
            Product product = null;
            _mockRepo.Setup(v => v.GetById(id)).ReturnsAsync(product);
            var result = await _controller.DeleteProduct(id);

            //result.Result => Becouse returns ActionResult<Product>
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteProduct_ActionExecute_ReturnNoContentResult(int id)
        {
            var product = _products.FirstOrDefault(v => v.Id == id);

            _mockRepo.Setup(v => v.GetById(id)).ReturnsAsync(product);

            _mockRepo.Setup(re => re.Delete(product));

            var result = await _controller.DeleteProduct(id);
            _mockRepo.Verify(v => v.Delete(It.IsAny<Product>()), Times.Once);

            var resturnResult = Assert.IsType<NoContentResult>(result.Result);
        }
    }
}
