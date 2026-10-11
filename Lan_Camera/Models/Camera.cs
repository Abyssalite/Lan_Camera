using System;

public class Camera
{
    public required string Address { get; set; }
    public required string Name { get; set; }
    public Guid ID = Guid.NewGuid();

    public Camera()
    {
    } 
}