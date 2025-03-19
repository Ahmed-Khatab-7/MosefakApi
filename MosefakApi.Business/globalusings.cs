global using AutoMapper;
global using Stripe;
global using MailKit.Net.Smtp;
global using System.Net.Http.Json;
global using MailKit.Security;
global using Microsoft.AspNetCore.DataProtection;
global using static MosefakApp.Infrastructure.constants.Permissions;
global using MosefakApp.Core.Dtos.Notification;
global using MosefakApp.Core.Dtos.Pagination;
global using MosefakApp.Core.Dtos.ChatBot.Responses;
global using MosefakApp.Core.Dtos.Review.Requests;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.UI.Services;
global using MosefakApp.Core.Dtos.ContactUs;
global using Microsoft.AspNetCore.WebUtilities;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Caching.Memory;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using MosefakApp.Core.IServices.FireBase;
global using Newtonsoft.Json;
global using MimeKit;
global using MosefakApp.Core.Dtos.Appointment.Requests;
global using MosefakApp.Core.Dtos.Appointment.Responses;
global using MosefakApp.Core.Dtos.Authentication;
global using MosefakApp.Core.Dtos.Authentication.Requests;
global using MosefakApp.Core.Dtos.Authentication.Responses;
global using MosefakApp.Core.Dtos.Clinic.Responses;
global using MosefakApp.Core.Dtos.Doctor.Requests;
global using MosefakApp.Core.Dtos.Doctor.Responses;
global using MosefakApp.Core.Dtos.Period.Responses;
global using MosefakApp.Core.Dtos.Review.Responses;
global using MosefakApp.Core.Dtos.Role.Request;
global using MosefakApp.Core.Dtos.Role.Responses;
global using MosefakApp.Core.Dtos.Schedule.Responses;
global using MosefakApp.Core.Dtos.Specialization.Responses;
global using MosefakApp.Core.Dtos.User.Requests;
global using MosefakApp.Core.Dtos.User.Responses;
global using MosefakApp.Core.IRepositories.Non_Generic;
global using MosefakApp.Core.IServices;
global using MosefakApp.Core.IServices.Authentication;
global using MosefakApp.Core.IServices.Cache;
global using MosefakApp.Core.IServices.Data_Protection;
global using MosefakApp.Core.IServices.Email;
global using MosefakApp.Core.IServices.Image;
global using MosefakApp.Core.IServices.Logging;
global using MosefakApp.Core.IServices.Role;
global using MosefakApp.Core.IServices.User;
global using MosefakApp.Core.IUnit;
global using MosefakApp.Domains.Entities;
global using MosefakApp.Domains.Entities.Identity;
global using MosefakApp.Domains.Enums;
global using MosefakApp.Infrastructure.constants;
global using MosefakApp.Infrastructure.Identity.context;
global using MosefakApp.Shared.Exceptions.Base;
global using System.ComponentModel.DataAnnotations;
global using System.IdentityModel.Tokens.Jwt;
global using System.Security.Claims;
global using System.Text;
global using System.Text.Json;
global using System.Transactions;
global using BadRequest = MosefakApp.Shared.Exceptions.Base.BadRequest;
global using MosefakApp.Core.IServices.Stripe;
global using System.Linq.Expressions;
global using Address = MosefakApp.Domains.Entities.Identity.Address;
global using MosefakApp.Core.Dtos.Award.Requests;
global using MosefakApp.Core.Dtos.Clinic.Requests;
global using MosefakApp.Core.Dtos.Education.Requests;
global using MosefakApp.Core.Dtos.Experience.Requests;
global using MosefakApp.Core.Dtos.Schedule.Requests;
global using MosefakApp.Core.Dtos.Specialization.Requests;
global using MosefakApp.Infrastructure.Repositories.Non_Generic;
global using Review = MosefakApp.Domains.Entities.Review;
global using MosefakApp.Core.Dtos.Education.Responses;
global using MosefakApp.Core.Dtos.Award.Responses;
global using MosefakApp.Core.Dtos.AppointmentType.Requests;
global using MosefakApp.Core.Dtos.Experience.Responses;










