using TravelTipsAPI.Constants;
using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Prefer Routes
    /// </summary>
    /// <param name="context">context</param>
    public class PreferRoutesService(
        TravelTipsContext context,
        IAttractionsService attractionsService
    ) : IPreferRoutesService
    {
        // prefer routes

        /// <summary>
        /// Find a route type by id
        /// </summary>
        /// <param name="id">route type id</param>
        /// <returns>the route type with id</returns>
        public PreferRoute FindPreferRouteById(int id)
        {
            var preferRoute = context.PreferRoutes.Find(id);

            if (preferRoute == null)
                throw new Exception(Messages.PreferRouteNotFound);

            return preferRoute;
        }

        /// <summary>
        /// Ge a list of prefer routes by search params
        /// </summary>
        /// <param name="type">prefer route type</param>
        /// <param name="departAttractionId">prefer route depart attraction id</param>
        /// <param name="arrivalAttractionId">prefer route arrival attraction id</param>
        /// <param name="estimateTimeMin">prefer route min estimate time</param>
        /// <param name="estimateTimeMax">prefer route max estimate time</param>
        /// <param name="ownerId">user id</param>
        /// <returns>a list of prefer routes that satisfy the search params</returns>
        public IEnumerable<PreferRouteViewModel> GetPreferRoutesByParams(
            int? type,
            long? departAttractionId,
            long? arrivalAttractionId,
            int? estimateTimeMin,
            int? estimateTimeMax,
            int? ownerId
        )
        {
            if (estimateTimeMin >= estimateTimeMax)
                throw new Exception(Messages.EstimateTimeMinMaxRestricted);

            IEnumerable<PreferRoute> preferRoutes = [.. context.PreferRoutes];

            if (type != null)
                preferRoutes = preferRoutes.Where(pr => pr.Type == type);
            if (departAttractionId != null)
                preferRoutes = preferRoutes.Where(pr =>
                    pr.DepartAttractionId == departAttractionId
                );
            if (arrivalAttractionId != null)
                preferRoutes = preferRoutes.Where(pr =>
                    pr.ArrivalAttractionId == arrivalAttractionId
                );
            if (estimateTimeMin != null)
                preferRoutes = preferRoutes.Where(pr => pr.EstimateTime >= estimateTimeMin);
            if (estimateTimeMax != null)
                preferRoutes = preferRoutes.Where(pr => pr.EstimateTime <= estimateTimeMax);
            if (ownerId != null)
                preferRoutes = preferRoutes.Where(pr => pr.CreatedBy == ownerId);

            // append Route Type view model to Type attribute
            var preferRouteViewModels = preferRoutes
                .Select(pr => new PreferRouteViewModel
                {
                    Id = pr.Id,
                    Type = (RouteTypeViewModel)FindRouteTypeById(pr.Type),
                    DepartAttraction = (AttractionViewModel)
                        attractionsService.GetAttractionById(pr.DepartAttractionId),
                    ArrivalAttraction = (AttractionViewModel)
                        attractionsService.GetAttractionById(pr.ArrivalAttractionId),
                    EstimateTime = pr.EstimateTime,
                    LinkId = pr.LinkId,
                    CreatedBy = pr.CreatedBy,
                })
                .ToList();

            return preferRouteViewModels;
        }

        /// <summary>
        /// Get my prefer route ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of my prefer route ids</returns>
        public IEnumerable<int> GetMyPreferRoutes(int id)
        {
            var myPreferRouteIds = context
                .PreferRoutes.Where(pr => pr.CreatedBy == id)
                .Select(pr => pr.Id)
                .ToList();

            return myPreferRouteIds;
        }

        /// <summary>
        /// Create a new prefer route
        /// </summary>
        /// <param name="createdBy">user id</param>
        /// <param name="preferRoutePostViewModel">new prefer route details</param>
        /// <returns>the new prefer route</returns>
        public async Task<PreferRouteViewModel> PostPreferRoutesAsync(
            int createdBy,
            PreferRoutePostViewModel preferRoutePostViewModel
        )
        {
            var newPreferRoute = preferRoutePostViewModel.ToPreferRoute(createdBy);

            await context.PreferRoutes.AddAsync(newPreferRoute);
            await context.SaveChangesAsync();

            var preferRouteViewModel = (PreferRouteViewModel)newPreferRoute;
            preferRouteViewModel.Type = (RouteTypeViewModel)FindRouteTypeById(newPreferRoute.Type);

            return preferRouteViewModel;
        }

        /// <summary>
        /// Update an existing prefer route you own
        /// </summary>
        /// <param name="preferRoute">prefer route</param>
        /// <param name="preferRoutePatchViewModel">the prefer route details to be updated</param>
        /// <returns>the prefer route up to date</returns>
        public async Task<PreferRouteViewModel> PatchPreferRoutesAsync(
            PreferRoute preferRoute,
            PreferRoutePatchViewModel preferRoutePatchViewModel
        )
        {
            preferRoute.Type = preferRoutePatchViewModel.Type ?? preferRoute.Type;
            preferRoute.DepartAttractionId =
                preferRoutePatchViewModel.DepartAttraction?.Id ?? preferRoute.DepartAttractionId;
            preferRoute.ArrivalAttractionId =
                preferRoutePatchViewModel.ArrivalAttraction?.Id ?? preferRoute.ArrivalAttractionId;
            preferRoute.EstimateTime =
                preferRoutePatchViewModel.EstimateTime ?? preferRoute.EstimateTime;
            preferRoute.LinkId = preferRoutePatchViewModel.LinkId ?? preferRoute.LinkId;
            preferRoute.IsDeprecated = false;

            await context.SaveChangesAsync();

            var preferRouteViewModel = (PreferRouteViewModel)preferRoute;
            preferRouteViewModel.Type = (RouteTypeViewModel)FindRouteTypeById(preferRoute.Type);

            return preferRouteViewModel;
        }

        /// <summary>
        /// mark prefer routes with a certain attraction to be deprecated
        /// </summary>
        /// <param name="attractionId">the attraction id</param>
        /// <returns>the number of prefer routes deprecated</returns>
        public async Task<int> PatchPreferRoutesDeprecated(int attractionId)
        {
            var preferRoutes = context.PreferRoutes.Where(pr =>
                (pr.DepartAttractionId == attractionId || pr.ArrivalAttractionId == attractionId)
                && pr.IsDeprecated == false
            );

            foreach (var preferRoute in preferRoutes)
            {
                preferRoute.IsDeprecated = true;
            }

            await context.SaveChangesAsync();

            return preferRoutes.Count();
        }

        /// <summary>
        /// Delete a prefer route by its id
        /// </summary>
        /// <param name="preferRoute">prefer route</param>
        /// <returns>the prefer route deleted</returns>
        public async Task<PreferRouteViewModel> DeletePreferRoute(PreferRoute preferRoute)
        {
            if (GetTaoInUse(preferRoute.Id) > 0)
                throw new Exception(Messages.PreferRouteInUse);

            var preferRouteViewModel = ToViewModel(preferRoute);

            context.PreferRoutes.Remove(preferRoute);
            await context.SaveChangesAsync();

            return preferRouteViewModel;
        }

        /// <summary>
        /// Get the number of trip attraction orders using the prefer route with the id
        /// </summary>
        /// <param name="id">prefer route id</param>
        /// <returns>the number of taos in use</returns>
        private int GetTaoInUse(int id)
        {
            return context
                .TripAttractionOrderRoutes.Where(taor => taor.PreferRouteId == id)
                .Count();
        }

        // route types

        /// <summary>
        /// Find a route type by its id
        /// </summary>
        /// <param name="id">route type id</param>
        /// <returns>the route type with the id</returns>
        public RouteType FindRouteTypeById(int id)
        {
            var routeType = context.RouteTypes.Find(id);

            if (routeType == null)
                throw new Exception(Messages.RouteTypeNotFound);

            return routeType;
        }

        /// <summary>
        /// Get a list of all route types
        /// </summary>
        /// <returns>the list of all route types</returns>
        public IEnumerable<RouteTypeViewModel> GetAllRouteTypes()
        {
            var routeTypeViewModels = context
                .RouteTypes.Select(rt => (RouteTypeViewModel)rt)
                .ToList();

            return routeTypeViewModels;
        }

        /// <summary>
        /// Create a new route type
        /// </summary>
        /// <param name="name">route type name</param>
        /// <returns>the new route type</returns>
        public async Task<RouteTypeViewModel> PostNewRouteTypeAsync(string name)
        {
            var newRouteType = new RouteType { Id = new int(), Name = name.Trim() };

            await context.RouteTypes.AddAsync(newRouteType);
            await context.SaveChangesAsync();

            return (RouteTypeViewModel)newRouteType;
        }

        /// <summary>
        /// Update an existing route type
        /// </summary>
        /// <param name="routeType">route type</param>
        /// <param name="name">route type name to be updated</param>
        /// <returns>the route type up to date</returns>
        public async Task<RouteTypeViewModel> PatchRouteTypeAsync(RouteType routeType, string name)
        {
            routeType.Name = name.Trim();

            await context.SaveChangesAsync();

            return (RouteTypeViewModel)routeType;
        }

        // utils

        public PreferRouteViewModel ToViewModel(PreferRoute preferRoute)
        {
            var preferRouteViewModel = (PreferRouteViewModel)preferRoute;
            preferRouteViewModel.Type = (RouteTypeViewModel)FindRouteTypeById(preferRoute.Type);

            return preferRouteViewModel;
        }

        /// <summary>
        /// Check if the name is under size
        /// </summary>
        /// <param name="name">route type name</param>
        /// <returns>true if is valid, false otherwise</returns>
        public List<string> ValidateNameChange(string name)
        {
            var invalidParams = new List<string>();

            if (name.Length > 20)
                invalidParams.Add("name");

            return invalidParams;
        }
    }
}
