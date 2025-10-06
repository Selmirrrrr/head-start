namespace HeadStart.WebAPI.Data;

public interface IAuditable
{
    Audit Audit { get; set; }
}

public class Audit
{
    public DateTime CreeLe { get; set; }
    public Guid? CreePar { get; set; }
    public DateTime ModifieLe { get; set; }
    public Guid? ModifiePar { get; set; }
}
