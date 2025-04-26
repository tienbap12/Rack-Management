using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Enum;
using System;
using System.ComponentModel.DataAnnotations.Schema; // Để sử dụng [ForeignKey]

namespace Rack.Domain.Entities;

/// <summary>
///Đại diện cho một linh kiện phần cứng có thể thay thế hoặc quản lý riêng lẻ.
/// Ví dụ: RAM, CPU, Disk Drive (HDD/SSD), Power Supply Unit (PSU), Network Interface Card (NIC), RAID Controller, Fan,...
/// </summary>
public class Component : Entity, IAuditInfo, ISoftDelete
{
    /// <summary>
    /// Tên mô tả của linh kiện, giúp dễ nhận biết.
    /// Ví dụ: "RAM DDR4 16GB 2933MHz ECC RDIMM", "Intel Xeon Gold 6248R", "SSD NVMe 1TB Samsung PM983"
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Loại linh kiện chính. Nên sử dụng một tập hợp giá trị được chuẩn hóa hoặc Enum nếu có thể.
    /// Ví dụ: "RAM", "CPU", "SSD", "HDD", "PSU", "NIC", "RAID_Card", "Fan", "Motherboard", "GPU"
    /// </summary>
    public string ComponentType { get; set; } = null!;

    /// <summary>
    /// Trạng thái hiện tại của linh kiện (Available, InUse, Faulty,...).
    /// </summary>
    public ComponentStatus Status { get; set; } = ComponentStatus.Available; // Mặc định là có sẵn khi tạo

    /// <summary>
    /// Số serial duy nhất của linh kiện (Optional nhưng rất nên có để theo dõi từng cái).
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// ID của DeviceRack (ví dụ: tủ I-06) nơi linh kiện được lưu trữ khi trạng thái là 'Available'.
    /// Sẽ là NULL nếu linh kiện đang được sử dụng (InUse) hoặc không có vị trí lưu trữ cố định.
    /// </summary>
    public Guid? StorageRackID { get; set; }

    /// <summary>
    /// ID của Device mà linh kiện này hiện đang được gắn vào (chỉ khi Status = InUse).
    /// Sẽ là NULL nếu linh kiện đang ở trạng thái Available, Faulty, Retired,...
    /// </summary>
    public Guid? CurrentDeviceID { get; set; }

    /// <summary>
    /// Tham chiếu đến DeviceRack nơi linh kiện được lưu trữ (khi chưa dùng).
    /// </summary>
    [ForeignKey(nameof(StorageRackID))]
    public virtual DeviceRack? StorageRack { get; set; } // Sử dụng virtual nếu dùng Lazy Loading

    /// <summary>
    /// Tham chiếu đến Device đang sử dụng linh kiện này.
    /// </summary>
    [ForeignKey(nameof(CurrentDeviceID))]
    public virtual Device? CurrentDevice { get; set; } // Sử dụng virtual nếu dùng Lazy Loading

    public bool IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = null!; // Đảm bảo không null khi tạo
    public DateTime? LastModifiedOn { get; set; }
    public string? LastModifiedBy { get; set; }

    // Có thể thêm constructor nếu cần logic khởi tạo phức tạp
    public Component()
    { }

    public Component(string name, string componentType)
    {
        Name = name;
        ComponentType = componentType;
        Status = ComponentStatus.Available; // Mặc định
    }
}