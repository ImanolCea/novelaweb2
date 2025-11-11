using System;
using System.Collections.Generic;

namespace novelaweb2.Models;

public partial class UserFollow
{
    public int IdSeguidor { get; set; }

    public int IdSeguido { get; set; }

    public DateTime FechaSeguimiento { get; set; }

    public virtual Usuario IdSeguidoNavigation { get; set; } = null!;

    public virtual Usuario IdSeguidorNavigation { get; set; } = null!;
}
