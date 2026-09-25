namespace GeneticSearch
{
    struct Protein
    {
        private string protein_name;
        private string organism_name;
        private string code_amino_acid;

        public string ProteinName
        {
            get { return protein_name; }
            set { protein_name = value ?? string.Empty; }
        }

        public string OrganismName
        {
            get { return organism_name; }
            set { organism_name = value ?? string.Empty; }
        }

        public string CodeAminoAcid
        {
            get { return code_amino_acid; }
            set { code_amino_acid = value ?? string.Empty; }
        }
    }
}
