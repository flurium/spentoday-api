using Backend.Services;
using Data;
using Data.Models.ProductTables;
using Lib;
using Lib.EntityFrameworkCore;
using Lib.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[Route("v1/test")]
[ApiController]
public class TestController : ControllerBase
{
}