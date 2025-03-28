using System;
using System.Collections.Generic;

namespace TravelTipsAPI.Models.Basic;

public partial class User
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<Attraction> Attractions { get; set; } = new List<Attraction>();

    public virtual ICollection<PreferRoute> PreferRoutes { get; set; } = new List<PreferRoute>();

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
