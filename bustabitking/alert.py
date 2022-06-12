import smtplib
from email.mime.text import MIMEText
from email.mime.image import MIMEImage
from email.mime.multipart import MIMEMultipart
import traceback

email_conf = {
    'imap': '',
    'smtp': '',
    'user': '',
    'pass': '',
    'received': '',
}

def sendmail(level, content, image_path=None):
    message = MIMEMultipart()
    message.attach(MIMEText(content))
    if image_path:
        imageApart = MIMEImage(open(image_path, 'rb').read(), image_path.split('.')[-1])
        imageApart.add_header('Content-Disposition', 'attachment', filename=image_path)
        message.attach(imageApart)
    message['From'] = email_conf['user']
    message['To'] = email_conf['received']
    message['Subject'] = f'Zommoros {level}'

    try:    
        smtpobj = smtplib.SMTP_SSL(email_conf['smtp'])
        smtpobj.login(email_conf['user'], email_conf['pass'])
        smtpobj.sendmail(email_conf['user'], [email_conf['received']], message.as_string())
        print('邮件发送成功!')
    except Exception:
        traceback.print_exc()
        print(f'无法发送邮件, 重试中......')
        sendmail(level, content)
