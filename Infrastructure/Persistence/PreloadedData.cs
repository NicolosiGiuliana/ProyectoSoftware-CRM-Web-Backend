using Application.Response;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Numerics;

namespace Infrastructure.Persistence
{
    public static class PreloadedData
    {
        public static void Preload(ModelBuilder modelBuilder)
        {
            //Precarga de datos para TP2
            modelBuilder.Entity<Client>().HasData(
                new Client { ClientID = 1, Name = "Gustavo Nicolosi", Email = "gustavo@nicolosi.com", Phone = "1123456789", Company = "X", Address = "123" , CreateDate = DateTime.Now },
                new Client { ClientID = 2, Name = "Cristina Espina", Email = "cristina@espina.com", Phone = "1176543210", Company = "Instagram", Address = "234", CreateDate = DateTime.Now },
                new Client { ClientID = 3, Name = "Giuliana Nicolosi", Email = "giuliana@nicolosi.com", Phone = "1134567890", Company = "Tiktok", Address = "567", CreateDate = DateTime.Now },
                new Client { ClientID = 4, Name = "Fiorella Nicolosi", Email = "fiorella@nicolosi.com", Phone = "1145678901", Company = "Pinterest", Address = "890", CreateDate = DateTime.Now },
                new Client { ClientID = 5, Name = "Isabella Nicolosi", Email = "isabellao@nicolosi.com", Phone = "1156789012", Company = "Youtube", Address = "987", CreateDate = DateTime.Now }
            );

            // Precarga de datos para las tablas iniciales
            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, Name = "Joe Done", Email = "jdone@marketing.com" },
                new User { UserID = 2, Name = "Nill Amstrong", Email = "namstrong@marketing.com" },
                new User { UserID = 3, Name = "Marlyn Morales", Email = "mmorales@marketing.com" },
                new User { UserID = 4, Name = "Antony Orué", Email = "aorue@marketing.com" },
                new User { UserID = 5, Name = "Jazmin Fernandez", Email = "jfernandez@marketing.com" }
            );

            modelBuilder.Entity<Domain.Entities.TaskStatus>().HasData(
                new Domain.Entities.TaskStatus { Id = 1, Name = "Pending" },
                new Domain.Entities.TaskStatus { Id = 2, Name = "In Progress" },
                new Domain.Entities.TaskStatus { Id = 3, Name = "Blocked" },
                new Domain.Entities.TaskStatus { Id = 4, Name = "Done" },
                new Domain.Entities.TaskStatus { Id = 5, Name = "Cancel" }
            );

            modelBuilder.Entity<InteractionType>().HasData(
                new InteractionType { Id = 1, Name = "Initial Meeting" },
                new InteractionType { Id = 2, Name = "Phone call" },
                new InteractionType { Id = 3, Name = "Email" },
                new InteractionType { Id = 4, Name = "Presentation of Results" }
            );

            modelBuilder.Entity<CampaignType>().HasData(
                new CampaignType { Id = 1, Name = "SEO" },
                new CampaignType { Id = 2, Name = "PPC" },
                new CampaignType { Id = 3, Name = "Social Media" },
                new CampaignType { Id = 4, Name = "Email Marketing" }
            );
        }
    }
}