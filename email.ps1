function SendEmail{
    param (
        $bodyMessage
    )

    $to = "someone@email.com, someone2@email.com"
    $from = "fromSomeone@email.com"
    $smtpServer = "smtp.company.com"
    $subject = "email subject"
    $body = $bodyMessage

    Send-MailMessage -To $to -From $from -Subject $subject -Body $body -SmtpServer $smtpServer
}

SendEmail -bodyMessage "Bodymessage"