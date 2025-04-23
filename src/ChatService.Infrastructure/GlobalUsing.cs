// Package
global using System.Text.Json;
global using Confluent.Kafka;
global using System.Reflection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.DependencyInjection;
global using MediatR;
// Assembly
global using ChatService.Contract.EventBus.Abstractions;
global using ChatService.Contract.EventBus.Abstractions.Message;
global using ChatService.Contract.Settings;
global using ChatService.Infrastructure.EventBus;
global using ChatService.Infrastructure.EventBus.Kafka;
