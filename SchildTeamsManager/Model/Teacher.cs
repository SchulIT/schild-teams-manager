namespace SchildTeamsManager.Model
{
    public class Teacher
    {
        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string EmailAddress { get; set; }

        public override string ToString()
        {
            return $"{Lastname}, {Firstname}";
        }
    }
}
