try{
    $resp = Invoke-WebRequest -URI "https://something.com"

    If ($resp.StatusCode -eq '200'){
        # Request successfull

        exit
    }

    # Request failed
    

}catch{

    # Request failed
}