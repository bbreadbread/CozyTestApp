using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CozyTest.Models;

public partial class Group : ObservableObject
{
    GroupService _groupService = new();

    private int _id;
    private string? _name;
    private string? _description;
    private int _curatorId;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int? CuratorId { get; set; }

    public string? Description { get; set; }

    public virtual Curator? Curator { get; set; }

    public virtual List<Participant> Participants { get; set; } = new List<Participant>();

    ///////////////////
    
    [NotMapped]
    private bool _isPublished;
    [NotMapped]
    public bool IsPublished
    {
        get
        {
            _groupService.GetAllGroupsForCurator(CurrentUser.Id);

            _isPublished = true;
            return _isPublished;
        }
        set
        {
            if (SetProperty(ref _isPublished, value))
            {

            }
        }
    }
}
