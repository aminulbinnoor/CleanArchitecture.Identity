// Global using directives for Identity.API
global using System;
global using System.Collections.Generic;
global using System.Security.Claims;
global using System.Threading.Tasks;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Models;
global using MediatR;
global using Identity.Application.DTOs;
global using Identity.Application.Features.Auth.Commands.Login;
global using Identity.Application.Features.Auth.Commands.Logout;
global using Identity.Application.Features.Auth.Commands.RefreshToken;
global using Identity.Application.Features.Auth.Commands.Register;