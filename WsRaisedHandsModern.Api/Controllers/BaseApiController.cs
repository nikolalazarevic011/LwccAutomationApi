using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WsRaisedHandsModern.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseApiController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns a standardized success response
        /// </summary>
        protected IActionResult SuccessResponse<T>(T data, string message = "Operation completed successfully")
        {
            return Ok(new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            });
        }

        /// <summary>
        /// Returns a standardized error response
        /// </summary>
        protected IActionResult ErrorResponse(string message, int statusCode = 400)
        {
            var response = new ApiResponse<object>
            {
                Success = false,
                Message = message,
                Data = null
            };

            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Returns a standardized not found response
        /// </summary>
        protected IActionResult NotFoundResponse(string message = "Resource not found")
        {
            return ErrorResponse(message, 404);
        }

        /// <summary>
        /// Logs and returns an internal server error response
        /// </summary>
        protected IActionResult InternalServerErrorResponse(Exception ex, string message = "An internal error occurred")
        {
            _logger.LogError(ex, message);
            return ErrorResponse(message, 500);
        }

        /// <summary>
        /// Validates date range parameters
        /// </summary>
        protected bool IsValidDateRange(DateTime startDate, DateTime endDate, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (startDate > endDate)
            {
                errorMessage = "Start date cannot be greater than end date";
                return false;
            }

            if (startDate > DateTime.Now)
            {
                errorMessage = "Start date cannot be in the future";
                return false;
            }

            var daysDifference = (endDate - startDate).TotalDays;
            if (daysDifference > 365)
            {
                errorMessage = "Date range cannot exceed 365 days";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the current user's ID from the JWT token
        /// </summary>
        protected int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        /// <summary>
        /// Gets the current user's roles
        /// </summary>
        protected IEnumerable<string> GetCurrentUserRoles()
        {
            return User.FindAll("role").Select(c => c.Value);
        }
    }

    
    /// <summary>
    /// Standardized API response model
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}