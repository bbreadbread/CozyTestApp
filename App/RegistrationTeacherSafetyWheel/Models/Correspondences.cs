namespace RegistrationCuratorCozyTest.Models
{
    public class Сorrespondence
    {
        public int ConstantId { get; set; }      
        public int СorrespondingId { get; set; } 

        public virtual Option Constant { get; set; }
        public virtual Option Corresponding { get; set; }
    }
}
