namespace TravelTipsAPI.Services.TravelTipsServices;

using System.Collections.Generic;
using System.Threading.Tasks;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

/// <summary>
/// The service of Trip Attraction Orders
/// </summary>
/// <param name="context">travel tips context</param>
public class TripAttractionOrdersService(
    TravelTipsContext context
//IPreferRoutesService preferRoutesService,
) : ITripAttractionOrdersService
{
    /// <summary>
    /// Get all your trip attraction order ids
    /// </summary>
    /// <param name="id">user id</param>
    /// <returns>a list of your trip attraction order ids</returns>
    public IEnumerable<int> GetMyTaos(int id)
    {
        var myTaoIds = context
            .TripAttractionOrders.Where(tao => tao.CreatedBy == id)
            .Select(tao => tao.Id)
            .ToList();

        return myTaoIds;
    }

    /// <summary>
    /// Find tao by id
    /// </summary>
    /// <param name="id">tao id</param>
    /// <returns>tao</returns>
    public TripAttractionOrder? FindTaoById(int id)
    {
        var tao = context.TripAttractionOrders.FirstOrDefault(tao => tao.Id == id);

        return tao;
    }

    /// <summary>
    /// Get a list of taos by day id
    /// </summary>
    /// <param name="dayId">day id</param>
    /// <returns>a list of taos in the day</returns>
    public IEnumerable<TripAttractionOrderViewModel> GetTaosByDayId(int dayId)
    {
        var taos = context.TripAttractionOrders.Where(tao => tao.DayId == dayId).ToList();

        var taoViewModels = taos.Select(tao => new TripAttractionOrderViewModel
            {
                Id = tao.Id,
                DayId = dayId,
                Start = tao.Start,
                End = tao.End,
                CreatedBy = tao.CreatedBy,
                Attraction = (Attraction2ViewModel)tao.Attraction,
                Highlight = tao.Highlight != null ? (HighlightViewModel)tao.Highlight : null,
            })
            .OrderBy(t => t.Start)
            .ToList();

        return taoViewModels;
    }

    /// <summary>
    /// Create a new tao
    /// </summary>
    /// <param name="newTao">new tao</param>
    /// <param name="userId">user id</param>
    /// <returns>the new tao id</returns>
    public async Task<int> PostTao(TripAttractionOrderPostViewModel newTao, int userId)
    {
        var tao = newTao.ToTripAttractionOrder(userId);

        await context.TripAttractionOrders.AddAsync(tao);
        await context.SaveChangesAsync();

        return tao.Id;
    }

    /// <summary>
    /// Update tao with updated tao details
    /// </summary>
    /// <param name="taoPatch">tao details to be updated</param>
    /// <param name="id">tao id</param>
    /// <returns>the updated tao id</returns>
    public async Task<int> PatchTao(
        TripAttractionOrderPatchViewModel taoPatch,
        TripAttractionOrder tao
    )
    {
        tao!.AttractionId = taoPatch.AttractionId ?? tao.AttractionId;
        tao.HighlightId = taoPatch.HighlightId ?? tao.HighlightId;
        tao.DayId = taoPatch.DayId ?? tao.DayId;
        tao.Start = taoPatch.Start ?? tao.Start;
        tao.End = taoPatch?.End ?? tao.End;

        await context.SaveChangesAsync();

        return tao.Id;
    }

    /// <summary>
    /// Delete a tao by tao id
    /// </summary>
    /// <param name="tao">tao</param>
    /// <returns>deleted tao id</returns>
    public async Task<int> DeleteTaoById(TripAttractionOrder tao)
    {
        context.TripAttractionOrders.Remove(tao);
        await context.SaveChangesAsync();

        return tao.Id;
    }

    /// <summary>
    /// Delete a list of taos by day id
    /// </summary>
    /// <param name="dayId">day id</param>
    /// <returns></returns>
    public async Task<int> DeleteTaosByDayId(int dayId)
    {
        var taos = context.TripAttractionOrders.Where(tao => tao.DayId == dayId).ToList();

        context.TripAttractionOrders.RemoveRange(taos);
        await context.SaveChangesAsync();

        return taos.Count;
    }

    /// <summary>
    /// Check if time is aligned to a 15-minute interval
    /// </summary>
    /// <param name="time">start/end time</param>
    public void IsTimeValid(TimeOnly time)
    {
        if (time.Minute % 15 == 0)
            return;

        throw new Exception(Messages.TaoTimeInvalid);
    }

    /// <summary>
    /// Check if tao start and end time overlaps with other taos in the same day
    /// </summary>
    /// <param name="start">start time</param>
    /// <param name="end">end time</param>
    /// <param name="dayId">day id</param>
    public void IsTaoConflicted(TimeOnly start, TimeOnly end, int dayId)
    {
        var existingTaos = context
            .TripAttractionOrders.Where(tao => tao.DayId == dayId)
            .Select(tao => new { tao.Start, tao.End })
            .ToList();

        foreach (var tao in existingTaos)
        {
            if (start < tao.End && tao.Start < end)
            {
                // Overlap detected
                throw new Exception(Messages.TaoTimeConflicted);
            }
        }
        return; // No overlaps
    }
}

//    /// <summary>
//    /// Get a trip attraction order
//    /// </summary>
//    /// <param name="id">trip attraction order id</param>
//    /// <param name="isPublic">is trip attraction order in a public trip</param>
//    /// <returns>the trip attraction order with the id</returns>
//    public TripAttractionOrder FindTripAttractionOrderById(int id, bool? isPublic = null)
//    {
//        var tao = context.TripAttractionOrders.Find(id);

//        if (tao == null)
//            throw new Exception(Messages.TaoNotFound);

//        if (isPublic != null)
//        {
//            var isTripPublic = context
//                .Days.Where(day => day.Id == tao.DayId)
//                .Select(day => day.Trip)
//                .Select(trip => trip.IsPublic)
//                .First();

//            if (isTripPublic != isPublic)
//                throw new Exception(Messages.TaoNotFound);
//        }

//        return tao;
//    }

//    /// <summary>
//    /// Get a list of trip attraction orders by day id
//    /// </summary>
//    /// <param name="dayId">day id</param>
//    /// <returns>a list of trip attraction orders of the day</returns>
//    public IEnumerable<TripAttractionOrder> GetTripAttractionOrdersByDayId(int dayId)
//    {
//        var taos = context
//            .TripAttractionOrders.Where(tao => tao.DayId == dayId)
//            .OrderBy(tao => tao.Order)
//            .ToList();
//        return taos;
//    }
//    /// <summary>
//    /// Create a new trip attraction order
//    /// </summary>
//    /// <param name="createdBy">user id</param>
//    /// <param name="taoPostViewModel">the new trip attraction order detail</param>
//    /// <returns>the new trip attraction order</returns>
//    public async Task<TripAttractionOrderViewModel> PostTripAttractionOrderAsync(
//        int createdBy,
//        TripAttractionOrderPostViewModel taoPostViewModel
//    )
//    {
//        // validate estimate time
//        if (taoPostViewModel.EstimateTime <= 0)
//            throw new Exception(Messages.EstimateTimeRestricted);

//        // validate estimate travel time
//        if (taoPostViewModel.EstimateTravelTime <= 0)
//            throw new Exception(Messages.EstimateTravelTimeRestricted);

//        var taosInSameDay = context
//            .TripAttractionOrders.Where(tao => tao.DayId == taoPostViewModel.DayId)
//            .ToList();

//        // append new trip attraction order to the end of the order list
//        var tao = taoPostViewModel.ToTripAttractionOrder(createdBy);
//        tao.Order = taosInSameDay.Count + 1;

//        var isOrderValid = IsOrderValid(taosInSameDay.Count + 1, taoPostViewModel.Order);
//        if (!isOrderValid)
//            throw new Exception(Messages.NewOrderInvalid);

//        await context.TripAttractionOrders.AddAsync(tao);
//        await context.SaveChangesAsync();

//        // update the orders of the taos in the same day
//        await SetOrderAsync(tao, taoPostViewModel.Order);

//        tao.Order = taoPostViewModel.Order;
//        return ToViewModel(tao);
//    }

//    /// <summary>
//    /// Update an existing trip attraction order
//    /// </summary>
//    /// <param name="tao">trip attraction order</param>
//    /// <param name="taoPatchViewModel">the trip attraction order details to be updated</param>
//    /// <returns>the trip attraction order up to date</returns>
//    public async Task<TripAttractionOrderViewModel> PatchTripAttractionOrderAsync(
//        TripAttractionOrder tao,
//        TripAttractionOrderPatchViewModel taoPatchViewModel
//    )
//    {
//        // validate estimate time
//        if (taoPatchViewModel.EstimateTime <= 0)
//            throw new Exception(Messages.EstimateTimeRestricted);

//        // validate estimate travel time
//        if (taoPatchViewModel.EstimateTravelTime <= 0)
//            throw new Exception(Messages.EstimateTravelTimeRestricted);

//        tao.DayId = tao.DayId;
//        tao.HighlightId = taoPatchViewModel.HighlightId ?? tao.HighlightId;
//        tao.EstimateTime = taoPatchViewModel.EstimateTime ?? tao.EstimateTime;
//        tao.EstimateTravelTime = taoPatchViewModel.EstimateTravelTime ?? tao.EstimateTravelTime;
//        tao.IsDrivePreferred = taoPatchViewModel.IsDrivePreferred ?? tao.IsDrivePreferred;
//        tao.IsBikePreferred = taoPatchViewModel.IsBikePreferred ?? tao.IsBikePreferred;
//        tao.IsOnFootPreferred = taoPatchViewModel.IsOnFootPreferred ?? tao.IsOnFootPreferred;

//        await context.SaveChangesAsync();

//        return ToViewModel(tao);
//    }

//    /// <summary>
//    /// Update the order of a trip attraction order and the consequent order change
//    /// </summary>
//    /// <param name="tao">trip attraction order</param>
//    /// <param name="newOrder">trip attraction order new order</param>
//    /// <returns>a list of trip attraction orders under the same day</returns>
//    public async Task<IEnumerable<TripAttractionOrderViewModel>> SetOrderAsync(
//        TripAttractionOrder tao,
//        int newOrder
//    )
//    {
//        var taosInSameDay = context
//            .TripAttractionOrders.Where(_tao => _tao.DayId == tao.DayId)
//            .OrderBy(_tao => _tao.Order)
//            .ToList();

//        if (taosInSameDay.Count > 1)
//        {
//            var isOrderValid = IsOrderValid(taosInSameDay.Count, newOrder);
//            if (!isOrderValid)
//                throw new Exception(Messages.NewOrderInvalid);

//            // Remove the tao by ID instead of index to avoid mismatch
//            taosInSameDay.RemoveAll(t => t.Id == tao.Id);

//            // Clamp the newOrder within range
//            newOrder = Math.Max(1, Math.Min(newOrder, taosInSameDay.Count + 1));

//            // Insert at the correct position (newOrder is 1-based, so subtract 1)
//            taosInSameDay.Insert(newOrder - 1, tao);

//            // Reassign orders starting from 1
//            for (int i = 0; i < taosInSameDay.Count; i++)
//            {
//                taosInSameDay[i].Order = i + 1;
//            }

//            await context.SaveChangesAsync();
//        }

//        var taoViewModels = taosInSameDay.Select(tao => ToViewModel(tao));

//        return taoViewModels;
//    }

//    /// <summary>
//    /// Remove a trip attraction order you own
//    /// </summary>
//    /// <param name="tao">trip attraction order</param>
//    /// <returns>the id of the deleted trip attraction order</returns>
//    public async Task<TripAttractionOrderViewModel> DeleteTripAttractionOrderAsync(
//        TripAttractionOrder tao
//    )
//    {
//        var taoViewModel = ToViewModel(tao);

//        // remove the trip attraction order routes
//        var taors = GetTripAttractionOrderRoutes(tao.Id);
//        context.TripAttractionOrderRoutes.RemoveRange(taors);

//        // remove the trip attraction order
//        context.TripAttractionOrders.Remove(tao);
//        await context.SaveChangesAsync();

//        return taoViewModel;
//    }

//    // taors

//    private IEnumerable<TripAttractionOrderRoute> GetTripAttractionOrderRoutes(int taoId)
//    {
//        var preferRoutes = context
//            .TripAttractionOrderRoutes.Where(taor => taor.TripAttractionOrderId == taoId)
//            .ToList();

//        return preferRoutes;
//    }

//    private IEnumerable<PreferRoute> GetPreferRoutes(int taoId)
//    {
//        var preferRoutes = context
//            .TripAttractionOrderRoutes.Where(taor => taor.TripAttractionOrderId == taoId)
//            .OrderBy(taor => taor.Order)
//            .Select(taor => taor.PreferRoute)
//            .ToList();

//        return preferRoutes;
//    }

//    /// <summary>
//    /// Find a taor with tao id and prefer route id
//    /// </summary>
//    /// <param name="taoId">trip attraction order id</param>
//    /// <param name="preferRouteId">prefer route id</param>
//    /// <returns>the taor with the id</returns>
//    public TripAttractionOrderRoute FindTripAttractionOrderRoute(int taoId, int preferRouteId)
//    {
//        var taor = context.TripAttractionOrderRoutes.Find(taoId, preferRouteId);

//        if (taor == null)
//            throw new Exception(Messages.TaorNotFound);

//        return taor;
//    }

//    /// <summary>
//    /// Create a new trip attraction order route
//    /// </summary>
//    /// <param name="id">trip attraction order id</param>
//    /// <param name="preferRouteId">prefer route id</param>
//    /// <param name="order">order</param>
//    /// <returns>the new trip attraction order where new trip attraction order route is</returns>
//    public async Task<TripAttractionOrderViewModel> PostNewTripAttractionOrderRouteAsync(
//        int id,
//        int preferRouteId,
//        int order
//    )
//    {
//        var toars = context
//            .TripAttractionOrderRoutes.Where(toar => toar.TripAttractionOrderId == id)
//            .ToList();

//        var newTaor = new TripAttractionOrderRoute
//        {
//            TripAttractionOrderId = id,
//            PreferRouteId = preferRouteId,
//            Order = toars.Count + 1,
//        };

//        // append new trip attraction order route to the end of the order list
//        var isOrderValid = IsOrderValid(toars.Count + 1, order);
//        if (!isOrderValid)
//            throw new Exception(Messages.NewOrderInvalid);

//        await context.TripAttractionOrderRoutes.AddAsync(newTaor);
//        await context.SaveChangesAsync();

//        // update the orders of the taors in the same tao
//        var taoViewModel = await SetPreferRouteOrderAsync(newTaor, order);

//        return taoViewModel;
//    }

//    /// <summary>
//    /// Update the order of a trip attraction order route and the consequent order change
//    /// </summary>
//    /// <param name="taor">trip attraction order route</param>
//    /// <param name="newOrder">new order</param>
//    /// <returns>the trip attraction order with updated prefer route order</returns>
//    public async Task<TripAttractionOrderViewModel> SetPreferRouteOrderAsync(
//        TripAttractionOrderRoute taor,
//        int newOrder
//    )
//    {
//        var taors = context
//            .TripAttractionOrderRoutes.Where(taor =>
//                taor.TripAttractionOrderId == taor.TripAttractionOrderId
//            )
//            .OrderBy(taor => taor.Order)
//            .ToList();

//        if (taors.Count > 1)
//        {
//            var isOrderValid = IsOrderValid(taors.Count, newOrder);
//            if (!isOrderValid)
//                throw new Exception(Messages.NewOrderInvalid);

//            // swap the index of taor
//            taors.RemoveAt(taor.Order - 1);
//            taors.Insert(newOrder - 1, taor);

//            // reorganize taors in the same tao
//            foreach (var (_taor, i) in taors.Select((taor, i) => (taor, i)))
//            {
//                _taor.Order = i + 1;
//            }

//            await context.SaveChangesAsync();
//        }

//        var tao = FindTripAttractionOrderById(taor.TripAttractionOrderId);
//        return ToViewModel(tao);
//    }

//    /// <summary>
//    /// Delete a trip attraction order route you own
//    /// </summary>
//    /// <param name="taor">trip attraction order route</param>
//    /// <returns>the trip attraction order where the trip attraction order route was</returns>
//    public async Task<TripAttractionOrderViewModel> DeleteTripAttractionOrderRouteAsync(
//        TripAttractionOrderRoute taor
//    )
//    {
//        context.TripAttractionOrderRoutes.Remove(taor);
//        await context.SaveChangesAsync();

//        var tao = FindTripAttractionOrderById(taor.TripAttractionOrderId);
//        return ToViewModel(tao);
//    }

//    // utils

//    /// <summary>
//    /// Map a Trip Attraction Order to its view model
//    /// </summary>
//    /// <param name="tao">trip attraction order</param>
//    /// <returns>trip attraction order view model</returns>
//    public TripAttractionOrderViewModel ToViewModel(TripAttractionOrder tao)
//    {
//        var preferRoutes = GetPreferRoutes(tao.Id);

//        var taoViewModel = new TripAttractionOrderViewModel
//        {
//            Id = tao.Id,
//            DayId = tao.DayId,
//            Order = tao.Order,
//            Attraction = attractionsService.ToAttractionViewModel(
//                attractionsService.FindHighlightById(tao.HighlightId)
//            ),
//            EstimateTime = tao.EstimateTime,
//            CreatedBy = tao.CreatedBy,
//            EstimateTravelTime = tao.EstimateTravelTime,
//            IsDrivePreferred = tao.IsDrivePreferred,
//            IsBikePreferred = tao.IsBikePreferred,
//            IsOnFootPreferred = tao.IsOnFootPreferred,
//            PreferRoutes = preferRoutes.Select(route => preferRoutesService.ToViewModel(route)),
//        };

//        return taoViewModel;
//    }

//    /// <summary>
//    /// Check if the order is in a valid range
//    /// </summary>
//    /// <param name="size">size of order</param>
//    /// <param name="order">the order</param>
//    /// <returns>true if the order is valid, false otherwise</returns>
//    public bool IsOrderValid(int size, int order)
//    {
//        return order >= 1 && order <= size;
//    }
//}
