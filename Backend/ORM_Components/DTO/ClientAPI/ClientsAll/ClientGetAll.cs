namespace ORM_Components.DTO.ClientAPI.ClientsAll
{
    public class ClientGetAll
    {
        public ClientSelectionSettings Settings { get; set; }

        public List<ClientInfo> Content { get; set; }

        public void ContentFill(List<ClientInfo> listOut)
        {
            Content = listOut;
        }
    }
}
