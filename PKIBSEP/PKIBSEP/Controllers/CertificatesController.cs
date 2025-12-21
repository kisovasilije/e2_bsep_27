using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKIBSEP.Dtos;
using PKIBSEP.Dtos.Certificates;
using PKIBSEP.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PKIBSEP.Controllers
{
    [ApiController]
    [Route("api/certificates")]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateIssuerService _issuerService;
        private readonly IMapper _mapper;
        private readonly ILogger<CertificatesController> _logger;

        private readonly ICaService caService;

        public CertificatesController(ICertificateIssuerService issuerService, IMapper mapper, ILogger<CertificatesController> logger, ICaService caService)
        {
            _issuerService = issuerService;
            _mapper = mapper;
            _logger = logger;
            this.caService = caService;
        }

        [HttpPost("create-root")]
        [Authorize(Policy = "adminPolicy")] // Admin only
        [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CertificateDto>> CreateRoot([FromBody] CreateRootDto request)
        {
            var currentUserId = GetCurrentCaUserId();

            var subject = Models.Mappers.X500NameMapper.ToX509Name(request.Subject);
            var keyUsageFlags = Crypto.X509.KeyUsageMapper.ToBouncyCastleFlags(request.KeyUsage);

            var cert = await _issuerService.CreateRootAsync(
                subject,
                request.ValidityDays,
                request.PathLenConstraint,
                keyUsageFlags,
                createdByAdminId: currentUserId);

            return Ok(_mapper.Map<CertificateDto>(cert));
        }

        [HttpPost("issue-intermediate")]
        [Authorize(Policy = "adminOrCaPolicy")] // Admin or CAUser
        [ProducesResponseType(typeof(CertificateDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<CertificateDto>> IssueIntermediate([FromBody] IssueIntermediateDto request)
        {
            var currentUserId = GetCurrentCaUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var isAdmin = userRole == "Admin";

            int targetCaUserId;
            if (isAdmin)
            {
                // Admin MUST specify targetCaUserId
                if (request.TargetCaUserId == null)
                    return BadRequest("Admin must specify TargetCaUserId");
                targetCaUserId = request.TargetCaUserId.Value;
            }
            else // CAUser
            {
                // CA User can issue to themselves OR another CA User (organization check in service layer)
                targetCaUserId = request.TargetCaUserId ?? currentUserId;
            }

            var subject = Models.Mappers.X500NameMapper.ToX509Name(request.Subject);
            var keyUsageFlags = Crypto.X509.KeyUsageMapper.ToBouncyCastleFlags(request.KeyUsage);

            try
            {
                var cert = await _issuerService.IssueIntermediateAsync(
                    request.IssuerId,
                    subject,
                    request.ValidityDays,
                    request.PathLenConstraint,
                    keyUsageFlags,
                    targetCaUserId,
                    assignedByUserId: currentUserId,
                    isAdmin: isAdmin);

                return Ok(_mapper.Map<CertificateDto>(cert));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        /// <summary>
        /// Returns CA certificates available to current user (for issuer selection)
        /// Admin sees ALL CA certificates, CA user sees ONLY their assigned chains
        /// </summary>
        [HttpGet("my-ca-certificates")]
        [Authorize(Policy = "adminOrCaPolicy")]
        public async Task<ActionResult<List<CertificateDto>>> GetMyCACertificates()
        {
            var currentUserId = GetCurrentCaUserId();
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var isAdmin = userRole == "Admin";

            List<Models.Certificate.Certificate> certificates;

            if (isAdmin)
            {
                // Admin vidi SVE CA sertifikate (root + intermediate)
                certificates = await _issuerService.GetAllCACertificatesAsync();
            }
            else
            {
                // CA korisnik vidi SAMO intermediate CA sertifikate iz svojih dodeljenih lanaca
                // Root CA sertifikati su eksplicitno isključeni iz bezbednosnih razloga
                certificates = await _issuerService.GetUserCACertificatesAsync(currentUserId);
            }

            return Ok(certificates.Select(c => _mapper.Map<CertificateDto>(c)).ToList());
        }

        // ---- helpers ----

        /// <summary>
        /// Resolves the current CA user's integer id from JWT claims.
        /// Expects "sub" to contain the numeric user id (as issued by AuthService).
        /// </summary>
        private int GetCurrentCaUserId()
        {
            var sub = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(sub, out var id))
                return id;

            throw new UnauthorizedAccessException("JWT 'sub' must contain an integer user id.");
        }

        [HttpPost("csr")]
        public async Task<ActionResult<CsrResponseDto>> SignCertificate(CertificateSigningRequestDto csr)
        {
            try
            {
                var (clientCertPem, caCertPem, serialNumberHex) = await caService.SignCsrAsync(csr);

                return Ok(new CsrResponseDto(
                    clientCertPem: clientCertPem,
                    caCertPem: caCertPem,
                    serialNumberHex: serialNumberHex));
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("issuers")]
        public async Task<ActionResult<IEnumerable<CaDto>>> GetCAs ()
        {
            var result = await caService.GetCAsAsync();
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to retrieve CA certificates." });
            }
        }
    }
}
