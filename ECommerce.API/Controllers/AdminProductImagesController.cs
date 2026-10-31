using ECommerce.Application.Common.Constants;
using ECommerce.Application.Common.Models;
using ECommerce.Application.DTOs.Products;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/admin/products/{productId:int}/images")]
[Authorize(Roles = Roles.Admin)]
public class AdminProductImagesController : ControllerBase
{
    private const long MaxRequestBytes =
        ProductImageRules.MaxFilesPerRequest * ProductImageRules.MaxFileBytes + 1024 * 1024;

    private readonly IProductImageService _service;

    public AdminProductImagesController(IProductImageService service) => _service = service;

    [HttpPost]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<List<ProductImageDto>>> Upload(
        int productId, [FromForm] List<IFormFile> files, CancellationToken ct)
    {
        var uploads = files
            .Select(f => new UploadedFile(f.Length, f.OpenReadStream))
            .ToList();

        var result = await _service.AddAsync(productId, uploads, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }


    [HttpPut("{imageId:int}/main")]
    public async Task<IActionResult> SetMain(int productId, int imageId, CancellationToken ct)
    {
        await _service.SetMainAsync(productId, imageId, ct);
        return NoContent();
    }

    [HttpDelete("{imageId:int}")]
    public async Task<IActionResult> Delete(int productId, int imageId, CancellationToken ct)
    {
        await _service.DeleteAsync(productId, imageId, ct);
        return NoContent();
    }
}