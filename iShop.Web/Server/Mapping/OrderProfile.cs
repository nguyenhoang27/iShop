﻿using System;
using System.Linq;
using AutoMapper;
using iShop.Web.Server.Commons.BaseClasses;
using iShop.Web.Server.Core.Models;
using iShop.Web.Server.Core.Resources;

namespace iShop.Web.Server.Mapping
{
    public class OrderProfile : BaseProfile
    {


        protected override void CreateMap()
        {
            CreateMap<Order, SavedOrderResource>();

            CreateMap<Order, OrderResource>()
                .ForMember(or => or.OrderedItems, opt => opt.MapFrom(p =>
                    p.OrderedItems.Select(pc => new TitleOrderItemResource() { Product = pc.Product.Name, Quantity = pc.Quantity })))
                .ForMember(or => or.Shipping, opt => opt.MapFrom(o => o.Shipping))
                .ForMember(or => or.Invoice, opt => opt.MapFrom(o => o.Invoice));

            CreateMap<Order, SavedOrderResource>()
                .ForMember(o => o.Id, opt => opt.Ignore())
                .ForMember(d => d.OrderedItems, opt => opt.Ignore());

            CreateMap<SavedOrderResource, Order>()
                .ForMember(o => o.Id, opt => opt.Ignore())
                .ForMember(o => o.OrderedItems, opt => opt.Ignore())
                .ForMember(o => o.Invoice, opt => opt.Ignore())
                .ForMember(o => o.Shipping, opt => opt.Ignore())
                .AfterMap((or, o) =>
                {
                    // Get the list of added Items
                    var addedOrderedItems = or.OrderedItems
                        .Where(oir => o.OrderedItems.All(oi => oi.ProductId != oir.ProductId))
                        .Select(oir =>
                            new OrderedItem() { ProductId = oir.ProductId, Quantity = oir.Quantity, OrderId = or.Id })
                        .ToList();

                    var removedOrderedItems =
                        o.OrderedItems.Where(oi => or.OrderedItems.Any(oir=>oir.ProductId!=oi.ProductId)).ToList();
                    foreach (var oi in removedOrderedItems)
                        o.OrderedItems.Remove(oi);

                    // Add it to the database
                    foreach (var oi in addedOrderedItems)
                        o.OrderedItems.Add(oi);
                });



        }
    }
}
