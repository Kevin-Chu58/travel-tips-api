namespace TravelTipsAPI.Services.TravelTipsServices;

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TravelTipsAPI.Constants;
using TravelTipsAPI.Models.TravelTipsModels;
using TravelTipsAPI.ViewModels.db_basic;
using TravelTipsAPI.ViewModels.HereMap;
using static TravelTipsAPI.Services.TravelTipsServices.BasicSchema;

/// <summary>
/// The service of Trip Attraction Orders
/// </summary>
/// <param name="context">travel tips context</param>
public class TripAttractionOrdersService(TravelTipsContext context) : ITripAttractionOrdersService
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

    public int BackTrackTripIdByTaoId(int taoId)
    {
        var tripId = context
            .TripAttractionOrders.Where(tao => tao.Id == taoId)
            .Include(tao => tao.Day)
            .Select(tao => tao.Day.TripId)
            .FirstOrDefault();
        return tripId;
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
    /// Get tao by id, if user is not owner or shared with, throw unauthorized exception
    /// </summary>
    /// <param name="id">tao id</param>
    /// <param name="isRestricted">is user owner or shared with</param>
    /// <returns>tao with the id</returns>
    public TripAttractionOrderViewModel GetTaoById(int id, bool isRestricted = false)
    {
        var tao = context
            .TripAttractionOrders.Include(t => t.Attraction)
            .Include(t => t.Highlight)
            .First(tao => tao.Id == id);

        if (!isRestricted && tao.IsPrivate)
            throw new Exception(Messages.TaoUnauthorized);

        var taoViewModel = GetTripAttractionOrderViewModel(tao);

        return taoViewModel;
    }

    /// <summary>
    /// Get a list of taos by day id, if user is not owner or shared with, only return public taos
    /// </summary>
    /// <param name="dayId">day id</param>
    /// <param name="isRestricted">is user owner or shared with</param>
    /// <returns>a list of taos in the day</returns>
    public IEnumerable<TripAttractionOrderViewModel> GetTaosByDayId(
        int dayId,
        bool isRestricted = false
    )
    {
        var taos = context.TripAttractionOrders.Where(tao => tao.DayId == dayId).ToList();

        var taoViewModels = taos.Select(tao => GetTripAttractionOrderViewModel(tao))
            .OrderBy(t => t.Start)
            .ToList();

        if (!isRestricted)
            taoViewModels = taoViewModels.Where(t => !t.IsPrivate).ToList();

        return taoViewModels;
    }

    /// <summary>
    /// Get a list of TaoGeo view models by day id, if user is not owner or shared with, only return public taos
    /// </summary>
    /// <param name="dayId">day id</param>
    /// <param name="isRestricted">is user owner or shared with</param>
    /// <returns>a list of TaoGeo view models</returns>
    public IEnumerable<TripAttractionOrderGeoViewModel> GetTaoGeosByDayId(
        int dayId,
        bool isRestricted = false
    )
    {
        var taos = context.TripAttractionOrders.Where(tao => tao.DayId == dayId).ToList();

        if (!isRestricted)
            taos = taos.Where(t => !t.IsPrivate).ToList();

        var taoViewModels = taos.Select(tao => new TripAttractionOrderGeoViewModel
            {
                Id = tao.Id,
                DayId = dayId,
                Title = tao.Attraction.Title,
                Lat = tao.Attraction.Lat,
                Lng = tao.Attraction.Lng,
            })
            .ToList();

        return taoViewModels;
    }

    /// <summary>
    /// Get tao geos by trip id, if user is not owner or shared with, only return public taos
    /// </summary>
    /// <param name="tripId">trip id</param>
    /// <param name="isRestricted">is user owner or shared with</param>
    /// <returns>a list of tao geos</returns>
    public IEnumerable<TripAttractionOrderGeoViewModel> GetTaoGeosByTripId(
        int tripId,
        bool isRestricted = false
    )
    {
        var taos = context
            .TripAttractionOrders.Where(t => t.Day.TripId == tripId)
            .Include(t => t.Attraction)
            .Include(t => t.Day)
            .ToList();

        if (!isRestricted)
            taos = taos.Where(t => !t.IsPrivate).ToList();

        var taoViewModels = taos.Select(tao => new TripAttractionOrderGeoViewModel
            {
                Id = tao.Id,
                DayId = tao.DayId,
                Title = tao.Attraction.Title,
                Lat = tao.Attraction.Lat,
                Lng = tao.Attraction.Lng,
            })
            .ToList();

        return taoViewModels;
    }

    /// <summary>
    /// Get here map routing input by tao id (the destination)
    /// </summary>
    /// <param name="taoId">tao id</param>
    /// <param name="isRestricted">is user owner or shared with</param>
    /// <returns>the here map routing input</returns>
    public HereRoutingInput? GetHereRoutingInputByTaoId(int taoId, bool isRestricted = false)
    {
        var taos = context
            .TripAttractionOrders.Where(tao =>
                context
                    .TripAttractionOrders.Where(inner => inner.Id == taoId)
                    .Select(inner => inner.DayId)
                    .Contains(tao.DayId)
            )
            .OrderBy(tao => tao.Start)
            .ToList();

        if (!isRestricted)
            taos = taos.Where(t => !t.IsPrivate).ToList();

        int taoIndex;

        try
        {
            taoIndex = taos.FindIndex(tao => tao.Id == taoId);
        }
        catch (Exception)
        {
            throw new Exception(Messages.TaoNotFound);
        }

        if (taoIndex == 0)
            return null;

        var tao = taos[taoIndex];
        var prevTao = taos[taoIndex - 1];

        var hereRoutingInput = new HereRoutingInput
        {
            TransportMode = tao.TransportMode ?? "car",
            OriginLat = (double)prevTao.Attraction.Lat,
            OriginLng = (double)prevTao.Attraction.Lng,
            DestinationLat = (double)tao.Attraction.Lat,
            DestinationLng = (double)tao.Attraction.Lng,
        };

        return hereRoutingInput;
    }

    /// <summary>
    /// Get a list of the here map routings of the day by day id
    /// </summary>
    /// <param name="dayId">day id</param>
    /// <param name="isRestricted">is user owner or shared with</param>
    /// <returns>a list of here map routing of the day</returns>
    public IEnumerable<HereRouting> GetAttractionRoutingsByDayId(
        int dayId,
        bool isRestricted = false
    )
    {
        var taos = context
            .TripAttractionOrders.Where(tao => tao.DayId == dayId)
            .OrderBy(tao => tao.Start)
            .ToList();

        if (!isRestricted)
            taos = taos.Where(t => !t.IsPrivate).ToList();

        var herePositions = taos.Select(tao => new HereRouting
            {
                Position = new HerePosition
                {
                    Lat = (double)tao.Attraction.Lat,
                    Lng = (double)tao.Attraction.Lng,
                },
                TransportMode = tao.TransportMode ?? "car",
            })
            .ToList();

        return herePositions;
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
    /// <param name="tao">tao</param>
    /// <returns>the updated tao id</returns>
    public async Task<int> PatchTao(
        TripAttractionOrderPatchViewModel taoPatch,
        TripAttractionOrder tao
    )
    {
        tao!.AttractionId = taoPatch.AttractionId ?? tao.AttractionId;
        tao.HighlightId =
            taoPatch.AttractionId != null ? null : taoPatch.HighlightId ?? tao.HighlightId;
        tao.DayId = taoPatch.DayId ?? tao.DayId;
        tao.Start = taoPatch.Start ?? tao.Start;
        tao.End = taoPatch?.End ?? tao.End;

        // check the validity of transport mode
        if (taoPatch?.TransportMode != null)
        {
            var isModeValid = HereMapEnum.ModeMap.TryGetValue(taoPatch.TransportMode, out var mode);

            if (!isModeValid)
                throw new Exception(Messages.HereMapTransportModeNotFound);
        }
        tao.TransportMode = taoPatch?.TransportMode ?? tao.TransportMode;

        await context.SaveChangesAsync();

        return tao.Id;
    }

    /// <summary>
    /// Detach highlight from tao
    /// </summary>
    /// <param name="tao">tao</param>
    /// <returns>tao id</returns>
    public async Task<int> PatchTaoDetachHighlight(TripAttractionOrder tao)
    {
        tao.HighlightId = null;
        await context.SaveChangesAsync();

        return tao.Id;
    }

    /// <summary>
    /// Update tao privacy setting
    /// </summary>
    /// <param name="tao">tao</param>
    /// <param name="isPrivate">is private</param>
    /// <returns>the updated privacy setting of the tao</returns>
    public async Task<bool> PatchTaoSetPrivate(TripAttractionOrder tao, bool isPrivate)
    {
        tao.IsPrivate = isPrivate;
        await context.SaveChangesAsync();
        return tao.IsPrivate;
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
    /// <param name="taoId">tao id to ignore</param>
    public void IsTaoConflicted(TimeOnly start, TimeOnly end, int dayId, int taoId = 0)
    {
        var existingTaos = context
            .TripAttractionOrders.Where(tao => tao.DayId == dayId && tao.Id != taoId)
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

    private TripAttractionOrderViewModel GetTripAttractionOrderViewModel(TripAttractionOrder tao)
    {
        var taoViewModel = new TripAttractionOrderViewModel
        {
            Id = tao.Id,
            DayId = tao.DayId,
            Start = tao.Start,
            End = tao.End,
            CreatedBy = tao.CreatedBy,
            Attraction = (AttractionViewModel)tao.Attraction,
            Highlight = tao.Highlight != null ? (HighlightViewModel)tao.Highlight : null,
            TransportMode = tao.TransportMode,
            IsPrivate = tao.IsPrivate,
        };
        return taoViewModel;
    }
}
