﻿---------------------------------------------------
-- EventNotifications
--------------------------------------------------

CREATE TRIGGER dbo.TRIGGER_EventNotifications_DELETE
ON dbo.EventNotifications
AFTER DELETE
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM dbo.Notifications WHERE Id IN(SELECT NotificationId FROM deleted);
END