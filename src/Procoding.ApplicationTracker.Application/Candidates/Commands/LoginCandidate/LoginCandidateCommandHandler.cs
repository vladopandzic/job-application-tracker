using FluentValidation;
using LanguageExt.Common;
using Microsoft.AspNetCore.Identity;
using Procoding.ApplicationTracker.Application.Authentication.Claims;
using Procoding.ApplicationTracker.Application.Authentication.JwtTokens;
using Procoding.ApplicationTracker.Application.Core.Abstractions.Messaging;
using Procoding.ApplicationTracker.Domain.Abstractions;
using Procoding.ApplicationTracker.Domain.Entities;
using Procoding.ApplicationTracker.Domain.Exceptions;
using Procoding.ApplicationTracker.Domain.Repositories;
using Procoding.ApplicationTracker.DTOs.Response.Candidates;

namespace Procoding.ApplicationTracker.Application.Candidates.Commands.LoginCandidate;

internal sealed class LoginCandidateCommandHandler : ICommandHandler<LoginCandidateCommand, CandidateLoginResponseDTO>
{

    readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<Candidate> _userManager;
    private readonly TimeProvider _timeProvider;
    private readonly IJwtTokenCreator<Candidate> _jwtTokenCreator;
    private readonly JwtTokenOptions<Candidate> _jwtTokenOptions;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCandidateCommandHandler(IRefreshTokenRepository refreshTokenRepository,
                                       UserManager<Candidate> userManager,
                                       TimeProvider timeProvider,
                                       IJwtTokenCreator<Candidate> jwtTokenCreator,
                                       JwtTokenOptions<Candidate> jwtTokenOptions,
                                       IUnitOfWork unitOfWork)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _timeProvider = timeProvider;
        _jwtTokenCreator = jwtTokenCreator;
        _jwtTokenOptions = jwtTokenOptions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CandidateLoginResponseDTO>> Handle(LoginCandidateCommand request, CancellationToken cancellationToken)
    {
        Candidate? candidate = await _userManager.FindByEmailAsync(request.Email);

        if (candidate is null || candidate.DeletedOnUtc is not null || await _userManager.CheckPasswordAsync(candidate, request.Password) == false)
        {
            return new Result<CandidateLoginResponseDTO>(new Unauthorized401Exception("Invalid username or password"));
        }

        // Gate: the account must have a confirmed email before it can sign in.
        if (!candidate.EmailConfirmed)
        {
            return new Result<CandidateLoginResponseDTO>(new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Email", "Vaš email još nije potvrđen. Provjerite inbox i spam za potvrdni link.")
            }));
        }

        var claims = ClaimsFactory.CreateClaims(userEmail: candidate.Email.ToString()!,
                                                userId: candidate.Id.ToString(),
                                                name: candidate.Name,
                                                surname: candidate.Surname);

        claims.AddRange(ClaimsFactory.CreateCandidateClaims());


        var expiryDate = _timeProvider.GetLocalNow().AddMonths(6);

        var tokenResponse = new CandidateLoginResponseDTO()
        {
            AccessToken = _jwtTokenCreator.CreateJwtToken(claims),
            RefreshToken = GenerateRefreshToken(),
            ExpiresIn = _jwtTokenOptions.ExpiresInSeconds,
            TokenType = "Bearer"
        };
        //TODO: candidateId column to refreshToken database!
        await _refreshTokenRepository.InsertAsync(new Domain.Auth.RefreshToken(expiryDate: expiryDate,
                                                                          accessToken: tokenResponse.AccessToken,
                                                                          refreshToken: tokenResponse.RefreshToken,
                                                                          employeeId: candidate.Id));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tokenResponse;
    }

    private string GenerateRefreshToken()
    {
        return Guid.NewGuid().ToString();
    }
}
