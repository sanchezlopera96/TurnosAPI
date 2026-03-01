namespace TurnosAPI.Domain.Entities;

public class Branch
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    private readonly List<Appointment> _appointments = new();

    private Branch() { }

    public Branch(string name, string address, string city)
    {
        Id = Guid.NewGuid();
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Branch name is required.");
        Address = !string.IsNullOrWhiteSpace(address) ? address : throw new ArgumentException("Address is required.");
        City = !string.IsNullOrWhiteSpace(city) ? city : throw new ArgumentException("City is required.");
        IsActive = true;
    }

    public void Update(string name, string address, string city)
    {
        Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentException("Branch name is required.");
        Address = !string.IsNullOrWhiteSpace(address) ? address : throw new ArgumentException("Address is required.");
        City = !string.IsNullOrWhiteSpace(city) ? city : throw new ArgumentException("City is required.");
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}