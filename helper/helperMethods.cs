namespace testingStuff.helper;

public static class HelperMethods{
    public static int getResponsePoolIndex(string[] responsePool, string response)
    {
        int index_to_find = -1;
        foreach (string item in responsePool)
        {
            if (item.ToLower().Contains(response.ToLower()))
            {
                index_to_find = Array.IndexOf(responsePool, item);
                return index_to_find;
            }
        }
        return index_to_find;
    }

    public static bool arrayContains(string[] responsePool, string response){
        foreach(string item in responsePool){
            if (item.ToLower().Contains(response.ToLower())){
                return true;
            }
        }
        return false;
    }
}