using TravelTipsAPI.Models;
using TravelTipsAPI.ViewModels.db_basic;
using static TravelTipsAPI.Services.BasicSchema;

namespace TravelTipsAPI.Services
{
    /// <summary>
    /// The service of Prefer Routes
    /// </summary>
    /// <param name="context">context</param>
    public class PreferRoutesService(TravelTipsContext context) : IPreferRoutesService
    {
        // prefer routes

        /// <summary>
        /// Ge a list of prefer routes by search params
        /// </summary>
        /// <param name="type">prefer route type</param>
        /// <param name="reference">prefer route ref</param>
        /// <param name="departOsmId">prefer route depart osm id</param>
        /// <param name="arrivalOsmId">prefer route arrival osm id</param>
        /// <param name="estimateTimeMin">prefer route min estimate time</param>
        /// <param name="estimateTimeMax">prefer route max estimate time</param>
        /// <param name="ownerId">user id</param>
        /// <returns>a list of prefer routes that satisfy the search params</returns>
        public IEnumerable<PreferRouteViewModel> GetPreferRoutesByParams(int? type, string? reference, 
            int? departOsmId, int? arrivalOsmId, int? estimateTimeMin, int? estimateTimeMax, int? ownerId)
        {
            reference = reference?.Trim().ToLower();
            if (estimateTimeMin >= estimateTimeMax)
                throw new Exception("Max estimate time is smaller than min estimate time.");

            IEnumerable<PreferRoute> preferRoutes = [.. context.PreferRoutes];

            if (type != null) preferRoutes = preferRoutes.Where(pr => pr.Type == type);
            if (reference != null) preferRoutes = preferRoutes.Where(pr => pr.Ref.ToLower().Contains(reference));
            if (departOsmId != null) preferRoutes = preferRoutes.Where(pr => pr.DepartOsmId == departOsmId);
            if (arrivalOsmId != null) preferRoutes = preferRoutes.Where(pr => pr.ArrivalOsmId == arrivalOsmId);
            if (estimateTimeMin != null) preferRoutes = preferRoutes.Where(pr => pr.EstimateTime >= estimateTimeMin);
            if (estimateTimeMax != null) preferRoutes = preferRoutes.Where(pr => pr.EstimateTime <= estimateTimeMax);
            if (ownerId != null) preferRoutes = preferRoutes.Where(pr => pr.CreatedBy == ownerId);

            // append Route Type view model to Type attribute
            var preferRouteViewModels = preferRoutes.Select(pr => new PreferRouteViewModel
                {
                    Id = pr.Id,
                    Type = GetRouteTypeById(pr.Type),
                    Ref = pr.Ref,
                    DepartOsmId = pr.DepartOsmId,
                    ArrivalOsmId = pr.ArrivalOsmId,
                    EstimateTime = pr.EstimateTime,
                    LinkId = pr.LinkId,
                    CreatedBy = pr.CreatedBy
                })
                .ToList();

            return preferRouteViewModels;
        }

        /// <summary>
        /// Get your prefer route ids
        /// </summary>
        /// <param name="id">user id</param>
        /// <returns>a list of your prefer route ids</returns>
        public IEnumerable<int> GetYourPreferRoutes(int id)
        {
            var yourPreferRouteIds = context.PreferRoutes
                .Where(pr => pr.CreatedBy == id)
                .Select(pr => pr.Id)
                .ToList();

            return yourPreferRouteIds;
        }

        /// <summary>
        /// Create a new prefer route
        /// </summary>
        /// <param name="createdBy">user id</param>
        /// <param name="preferRoutePostViewModel">new prefer route details</param>
        /// <returns>the new prefer route</returns>
        public async Task<PreferRouteViewModel> PostPreferRoutesAsync(int createdBy, PreferRoutePostViewModel preferRoutePostViewModel)
        {
            var newPreferRoute = preferRoutePostViewModel.ToPreferRoute(createdBy);

            await context.PreferRoutes.AddAsync(newPreferRoute);
            await context.SaveChangesAsync();

            var preferRouteViewModel = (PreferRouteViewModel)newPreferRoute;
            preferRouteViewModel.Type = GetRouteTypeById(newPreferRoute.Type);

            return preferRouteViewModel;
        }

        /// <summary>
        /// Update an existing prefer route you own
        /// </summary>
        /// <param name="id">prefer route id</param>
        /// <param name="preferRoutePatchViewModel">the prefer route details to be updated</param>
        /// <returns>the prefer route up to date</returns>
        public async Task<PreferRouteViewModel> PatchPreferRoutesAsync(int id, PreferRoutePatchViewModel preferRoutePatchViewModel)
        {
            var preferRoute = context.PreferRoutes.Find(id) ?? throw new Exception("Prefer Route not found.");

            // check preferRoute.Type in range
            bool isTypeValid = GetAllRouteTypes().Any(rt => rt.Id == preferRoutePatchViewModel.Type);
            if (!isTypeValid)
                throw new Exception("Prefer Route type invalid.");

            preferRoute.Type = preferRoutePatchViewModel.Type ?? preferRoute.Type;
            preferRoute.Ref = preferRoutePatchViewModel.Ref ?? preferRoute.Ref;
            preferRoute.DepartOsmId = preferRoutePatchViewModel.DepartOsmId ?? preferRoute.DepartOsmId;
            preferRoute.ArrivalOsmId = preferRoutePatchViewModel.ArrivalOsmId ?? preferRoute.ArrivalOsmId;
            preferRoute.EstimateTime = preferRoutePatchViewModel.EstimateTime ?? preferRoute.EstimateTime;
            preferRoute.LinkId = preferRoutePatchViewModel.LinkId ?? preferRoute.LinkId;

            await context.SaveChangesAsync();

            var preferRouteViewModel = (PreferRouteViewModel)preferRoute;
            preferRouteViewModel.Type = GetRouteTypeById(preferRoute.Type);

            return preferRouteViewModel;
        }

        // route types

        /// <summary>
        /// Get a route type by its id
        /// </summary>
        /// <param name="id">route type id</param>
        /// <returns>the route type with the id</returns>
        private RouteTypeViewModel GetRouteTypeById(int id)
        {
            var routeTypeViewModel = context.RouteTypes.Find(id) ?? throw new Exception("Route Type not found.");

            return (RouteTypeViewModel)routeTypeViewModel;
        }

        /// <summary>
        /// Get a list of all route types
        /// </summary>
        /// <returns>the list of all route types</returns>
        public IEnumerable<RouteTypeViewModel> GetAllRouteTypes()
        {
            var routeTypeViewModels = context.RouteTypes.Select(rt => (RouteTypeViewModel)rt).ToList();

            return routeTypeViewModels;
        }

        /// <summary>
        /// Create a new route type
        /// </summary>
        /// <param name="name">route type name</param>
        /// <returns>the new route type</returns>
        public async Task<RouteTypeViewModel> PostNewRouteTypeAsync(string name)
        {
            var newRouteType = new RouteType
            {
                Id = new int(),
                Name = name
            };

            await context.RouteTypes.AddAsync(newRouteType);
            await context.SaveChangesAsync();

            return (RouteTypeViewModel)newRouteType;
        }

        /// <summary>
        /// Update an existing route type
        /// </summary>
        /// <param name="id">route type id</param>
        /// <param name="name">route type name to be updated</param>
        /// <returns>the route type up to date</returns>
        public async Task<RouteTypeViewModel> PatchRouteTypeAsync(int id, string name)
        {
            var routeType = context.RouteTypes.Find(id) ?? throw new Exception("Route Type not found.");

            routeType.Name = name;

            await context.SaveChangesAsync();

            return (RouteTypeViewModel)routeType;
        }

    }
}
