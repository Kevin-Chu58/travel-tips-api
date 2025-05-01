namespace TravelTipsAPI.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

/// <summary>
/// The service of Trip Attraction Orders
/// </summary>
/// <param name="context">travel tips context</param>
public class TripAttractionOrdersService(
    TravelTipsContext context,
    IPreferRoutesService preferRoutesService
) : ITripAttractionOrdersService
{
    // taos

    /// <summary>
    /// Get a trip attraction order
    /// </summary>
    /// <param name="id">trip attraction order id</param>
    /// <param name="isPublic">is trip attraction order in a public trip</param>
    /// <returns>the trip attraction order with the id</returns>
    public TripAttractionOrder FindTripAttractionOrderById(int id, bool? isPublic = null)
    {
        var tao = context.TripAttractionOrders.Find(id);

        if (tao == null)
            throw new Exception(Messages.TaoNotFound);

        if (isPublic != null)
        {
            var isTripPublic = context
                .Days.Where(day => day.Id == tao.DayId)
                .Select(day => day.Trip)
                .Select(trip => trip.IsPublic)
                .First();

            if (isTripPublic != isPublic)
                throw new Exception(Messages.TaoNotFound);
        }

        return tao;
    }

    /// <summary>
    /// Get a list of trip attraction orders by day id
    /// </summary>
    /// <param name="dayId">day id</param>
    /// <returns>a list of trip attraction orders of the day</returns>
    public IEnumerable<TripAttractionOrder> GetTripAttractionOrdersByDayId(int dayId)
    {
        var taos = context
            .TripAttractionOrders.Where(tao => tao.DayId == dayId)
            .OrderBy(tao => tao.Order)
            .ToList();
        //var taoViewModels = new List<TripAttractionOrderViewModel>();
        //foreach (var tao in taos)
        //{
        //    taoViewModels.Add(ToViewModel(tao));
        //}
        return taos;
    }

    /// <summary>
    /// Get all your trip attraction order ids
    /// </summary>
    /// <param name="id">user id</param>
    /// <returns>a list of your trip attraction order ids</returns>
    public IEnumerable<int> GetMyTripAttractionOrders(int id)
    {
        var myTaoIds = context
            .TripAttractionOrders.Where(tao => tao.CreatedBy == id)
            .Select(tao => tao.Id)
            .ToList();

        return myTaoIds;
    }

    /// <summary>
    /// Create a new trip attraction order
    /// </summary>
    /// <param name="createdBy">user id</param>
    /// <param name="taoPostViewModel">the new trip attraction order detail</param>
    /// <returns>the new trip attraction order</returns>
    public async Task<TripAttractionOrderViewModel> PostTripAttractionOrderAsync(
        int createdBy,
        TripAttractionOrderPostViewModel taoPostViewModel
    )
    {
        // validate estimate time
        if (taoPostViewModel.EstimateTime <= 0)
            throw new Exception(Messages.EstimateTimeRestricted);

        var taosInSameDay = context
            .TripAttractionOrders.Where(tao => tao.DayId == taoPostViewModel.DayId)
            .ToList();

        // append new trip attraction order to the end of the order list
        var tao = taoPostViewModel.ToTripAttractionOrder(createdBy);
        tao.Order = taosInSameDay.Count + 1;

        var isOrderValid = IsOrderValid(taosInSameDay.Count + 1, taoPostViewModel.Order);
        if (!isOrderValid)
            throw new Exception(Messages.NewOrderInvalid);

        await context.TripAttractionOrders.AddAsync(tao);
        await context.SaveChangesAsync();

        // update the orders of the taos in the same day
        await SetOrderAsync(tao, taoPostViewModel.Order);

        tao.Order = taoPostViewModel.Order;
        return ToViewModel(tao);
    }

    /// <summary>
    /// Update an existing trip attraction order
    /// </summary>
    /// <param name="tao">trip attraction order</param>
    /// <param name="taoPatchViewModel">the trip attraction order details to be updated</param>
    /// <returns>the trip attraction order up to date</returns>
    public async Task<TripAttractionOrderViewModel> PatchTripAttractionOrderAsync(
        TripAttractionOrder tao,
        TripAttractionOrderPatchViewModel taoPatchViewModel
    )
    {
        // validate estimate time
        if (taoPatchViewModel.EstimateTime <= 0)
            throw new Exception(Messages.EstimateTimeRestricted);

        tao.DayId = taoPatchViewModel.DayId ?? tao.DayId;
        tao.AttractionId = taoPatchViewModel.AttractionId ?? tao.AttractionId;
        tao.EstimateTime = taoPatchViewModel.EstimateTime ?? tao.EstimateTime;
        tao.IsDrivePreferred = taoPatchViewModel.IsDrivePreferred ?? tao.IsDrivePreferred;
        tao.IsBikePreferred = taoPatchViewModel.IsBikePreferred ?? tao.IsBikePreferred;
        tao.IsOnFootPreferred = taoPatchViewModel.IsOnFootPreferred ?? tao.IsOnFootPreferred;

        await context.SaveChangesAsync();

        return ToViewModel(tao);
    }

    /// <summary>
    /// Update the order of a trip attraction order and the consequent order change
    /// </summary>
    /// <param name="tao">trip attraction order</param>
    /// <param name="newOrder">trip attraction order new order</param>
    /// <returns>a list of trip attraction orders under the same day</returns>
    public async Task<IEnumerable<TripAttractionOrderViewModel>> SetOrderAsync(
        TripAttractionOrder tao,
        int newOrder
    )
    {
        var taosInSameDay = context
            .TripAttractionOrders.Where(tao => tao.DayId == tao.DayId)
            .OrderBy(tao => tao.Order)
            .ToList();

        if (taosInSameDay.Count > 1)
        {
            var isOrderValid = IsOrderValid(taosInSameDay.Count, newOrder);
            if (!isOrderValid)
                throw new Exception(Messages.NewOrderInvalid);

            // swap the index of tao
            taosInSameDay.RemoveAt(tao.Order - 1);
            taosInSameDay.Insert(newOrder - 1, tao);

            // reorganize taos in the same day
            foreach (var (_tao, i) in taosInSameDay.Select((tao, i) => (tao, i)))
            {
                _tao.Order = i + 1;
            }

            await context.SaveChangesAsync();
        }

        var taoViewModels = taosInSameDay.Select(tao => ToViewModel(tao));

        return taoViewModels;
    }

    /// <summary>
    /// Remove a trip attraction order you own
    /// </summary>
    /// <param name="tao">trip attraction order</param>
    /// <returns>the id of the deleted trip attraction order</returns>
    public async Task<TripAttractionOrderViewModel> DeleteTripAttractionOrderAsync(
        TripAttractionOrder tao
    )
    {
        var taoViewModel = ToViewModel(tao);

        // remove the trip attraction order routes
        var taors = GetTripAttractionOrderRoutes(tao.Id);
        context.TripAttractionOrderRoutes.RemoveRange(taors);

        // remove the trip attraction order
        context.TripAttractionOrders.Remove(tao);
        await context.SaveChangesAsync();

        return taoViewModel;
    }

    // taors

    private IEnumerable<TripAttractionOrderRoute> GetTripAttractionOrderRoutes(int taoId)
    {
        var preferRoutes = context
            .TripAttractionOrderRoutes.Where(taor => taor.TripAttractionOrderId == taoId)
            .ToList();

        return preferRoutes;
    }

    private IEnumerable<PreferRoute> GetPreferRoutes(int taoId)
    {
        var preferRoutes = context
            .TripAttractionOrderRoutes.Where(taor => taor.TripAttractionOrderId == taoId)
            .OrderBy(taor => taor.Order)
            .Select(taor => taor.PreferRoute)
            .ToList();

        return preferRoutes;
    }

    /// <summary>
    /// Find a taor with tao id and prefer route id
    /// </summary>
    /// <param name="taoId">trip attraction order id</param>
    /// <param name="preferRouteId">prefer route id</param>
    /// <returns>the taor with the id</returns>
    public TripAttractionOrderRoute FindTripAttractionOrderRoute(int taoId, int preferRouteId)
    {
        var taor = context.TripAttractionOrderRoutes.Find(taoId, preferRouteId);

        if (taor == null)
            throw new Exception(Messages.TaorNotFound);

        return taor;
    }

    /// <summary>
    /// Create a new trip attraction order route
    /// </summary>
    /// <param name="id">trip attraction order id</param>
    /// <param name="preferRouteId">prefer route id</param>
    /// <param name="order">order</param>
    /// <returns>the new trip attraction order where new trip attraction order route is</returns>
    public async Task<TripAttractionOrderViewModel> PostNewTripAttractionOrderRouteAsync(
        int id,
        int preferRouteId,
        int order
    )
    {
        var toars = context
            .TripAttractionOrderRoutes.Where(toar => toar.TripAttractionOrderId == id)
            .ToList();

        var newTaor = new TripAttractionOrderRoute
        {
            TripAttractionOrderId = id,
            PreferRouteId = preferRouteId,
            Order = toars.Count + 1,
        };

        // append new trip attraction order route to the end of the order list
        var isOrderValid = IsOrderValid(toars.Count + 1, order);
        if (!isOrderValid)
            throw new Exception(Messages.NewOrderInvalid);

        await context.TripAttractionOrderRoutes.AddAsync(newTaor);
        await context.SaveChangesAsync();

        // update the orders of the taors in the same tao
        var taoViewModel = await SetPreferRouteOrderAsync(newTaor, order);

        return taoViewModel;
    }

    /// <summary>
    /// Update the order of a trip attraction order route and the consequent order change
    /// </summary>
    /// <param name="taor">trip attraction order route</param>
    /// <param name="newOrder">new order</param>
    /// <returns>the trip attraction order with updated prefer route order</returns>
    public async Task<TripAttractionOrderViewModel> SetPreferRouteOrderAsync(
        TripAttractionOrderRoute taor,
        int newOrder
    )
    {
        var taors = context
            .TripAttractionOrderRoutes.Where(taor =>
                taor.TripAttractionOrderId == taor.TripAttractionOrderId
            )
            .OrderBy(taor => taor.Order)
            .ToList();

        if (taors.Count > 1)
        {
            var isOrderValid = IsOrderValid(taors.Count, newOrder);
            if (!isOrderValid)
                throw new Exception(Messages.NewOrderInvalid);

            // swap the index of taor
            taors.RemoveAt(taor.Order - 1);
            taors.Insert(newOrder - 1, taor);

            // reorganize taors in the same tao
            foreach (var (_taor, i) in taors.Select((taor, i) => (taor, i)))
            {
                _taor.Order = i + 1;
            }

            await context.SaveChangesAsync();
        }

        var tao = FindTripAttractionOrderById(taor.TripAttractionOrderId);
        return ToViewModel(tao);
    }

    /// <summary>
    /// Delete a trip attraction order route you own
    /// </summary>
    /// <param name="taor">trip attraction order route</param>
    /// <returns>the trip attraction order where the trip attraction order route was</returns>
    public async Task<TripAttractionOrderViewModel> DeleteTripAttractionOrderRouteAsync(
        TripAttractionOrderRoute taor
    )
    {
        context.TripAttractionOrderRoutes.Remove(taor);
        await context.SaveChangesAsync();

        var tao = FindTripAttractionOrderById(taor.TripAttractionOrderId);
        return ToViewModel(tao);
    }

    // utils

    /// <summary>
    /// Map a Trip Attraction Order to its view model
    /// </summary>
    /// <param name="tao">trip attraction order</param>
    /// <returns>trip attraction order view model</returns>
    public TripAttractionOrderViewModel ToViewModel(TripAttractionOrder tao)
    {
        var preferRoutes = GetPreferRoutes(tao.Id);

        var taoViewModel = new TripAttractionOrderViewModel
        {
            Id = tao.Id,
            DayId = tao.DayId,
            Order = tao.Order,
            AttractionId = tao.AttractionId,
            EstimateTime = tao.EstimateTime,
            CreatedBy = tao.AttractionId,
            IsDrivePreferred = tao.IsDrivePreferred,
            IsBikePreferred = tao.IsBikePreferred,
            IsOnFootPreferred = tao.IsOnFootPreferred,
            PreferRoutes = preferRoutes.Select(route => preferRoutesService.ToViewModel(route)),
        };

        return taoViewModel;
    }

    /// <summary>
    /// Check if the order is in a valid range
    /// </summary>
    /// <param name="size">size of order</param>
    /// <param name="order">the order</param>
    /// <returns>true if the order is valid, false otherwise</returns>
    public bool IsOrderValid(int size, int order)
    {
        return order >= 1 && order <= size;
    }
}
