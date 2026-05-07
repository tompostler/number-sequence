namespace number_sequence.Models
{
    public sealed class ChiroInput
    {
        // Tracking / submission metadata
        public DateTimeOffset RowCreatedAt { get; set; }
        public string EmailSubmitter { get; set; }

        // Patient / visit
        public string PatientName { get; set; }
        public string OwnerName { get; set; }
        public DateTimeOffset DateOfService { get; set; }
        public string ToEmail { get; set; }
        public string[] CcEmails { get; set; }
        public string ClinicAbbreviation { get; set; }

        // Head
        public string HeadNotes { get; set; }
        public string HeadOcciput { get; set; }
        public string HeadTmj { get; set; }

        // Cervical — indexed as C1=[0] … C7=[6]
        public string CervicalNotes { get; set; }
        public string[] Cervical { get; set; }

        // Thoracic — indexed as T1=[0] … T{n}=[n-1]; canine n=13, equine n=18
        public string ThoracicNotes { get; set; }
        public string Sternum { get; set; }
        public string[] Thoracic { get; set; }

        // Ribs — indexed as R1=[0] … R{n}=[n-1]; canine n=13, equine n=18
        public string[] Ribs { get; set; }

        // Lumbar — indexed as L1=[0] … L{n}=[n-1]; canine n=7, equine n=6
        public string LumbarNotes { get; set; }
        public string[] Lumbar { get; set; }
        // Equine only: L3/L4=[0], L4/L5=[1], L5/L6=[2]; null for canine
        public string[] LumbarIntertransverse { get; set; }

        // Sacrum
        public string SacrumNotes { get; set; }
        public string SacrumBase { get; set; }
        public string SacrumApex { get; set; }

        // Pelvis
        public string PelvicNotes { get; set; }
        public string PelvicLeft { get; set; }
        public string PelvicRight { get; set; }
        public string PelvicTraction { get; set; }

        // Left forelimb
        public string LeftForelimbNotes { get; set; }
        public string LeftForelimbScapula { get; set; }
        public string LeftForelimbHumerus { get; set; }
        public string LeftForelimbUlna { get; set; }
        public string LeftForelimbRadius { get; set; }
        public string LeftForelimbCarpus { get; set; }
        public string LeftForelimbMetatarsalsPhalanges { get; set; }

        // Right forelimb
        public string RightForelimbNotes { get; set; }
        public string RightForelimbScapula { get; set; }
        public string RightForelimbHumerus { get; set; }
        public string RightForelimbUlna { get; set; }
        public string RightForelimbRadius { get; set; }
        public string RightForelimbCarpus { get; set; }
        public string RightForelimbMetatarsalsPhalanges { get; set; }

        // Left rear limb
        public string LeftRearLimbNotes { get; set; }
        public string LeftRearLimb { get; set; }

        // Right rear limb
        public string RightRearLimbNotes { get; set; }
        public string RightRearLimb { get; set; }

        // Coccygeal
        public string CoccygealNotes { get; set; }
        public string Coccygeal { get; set; }

        // Other
        public string Other { get; set; }
    }
}
