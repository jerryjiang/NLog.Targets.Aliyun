# NLog Target for Aliyun Log Service

A .NET Core NLog target to send log message to Aliyun Log Service.

## Install via NuGet
    Install-Package NLog.Targets.Aliyun

## Steps to Configure
1. Create a Log Service project and logstore
2. Enable RAM access control and create an AccessKey
3. Add **NLog.Targets.Aliyun** as an extension to your nlog.config
4. Configure endpoint, project, accessKeyId and accessKey in the nlog.config
5. Specify the logstore you want the log post to
   
   e.g., set logStore="\${lowercase:\${aspnet-environment}}", the log will send to **development**, **staging** or **production** logstore depends on the environment, some initialization logs will be dropped as environment not set yet

## Supported Configuration

- _**name**_ - Specify a name, but make sure you use the same name in logger rules section
- _**endpoint**_ - You can find it in the home screnn of your log service project. Use external endpoint if your application is not deployed within Aliyun
- _**project**_ - The log service project name you specified
- _**accessKeyId**_ - The RAM access key id
- _**accessKey**_ - The RAM access key
- _**logStore**_ - Support layout rendering, the log service log store name you specified
- _**source**_ - Support layout rendering, leave as empty send nothing to server while remove this attribute will send the IP address the application are running
- _**topic**_ - Support layout rendering, leave as empty send nothing to server while remove this attribute will send the logger name
- _**layout**_ - The message layout, this property will be ignored when context properties is set

## Configuration Examples

- Add NLog.Targets.Aliyun Extension
```xml
        <add assembly="NLog.Targets.Aliyun"/>
```

- Add Aliyun Target
```xml
        <target xsi:type="Aliyun" name="aliyun"
                endpoint="<your-endpoint>"
                project="<your-project-name>"
                accessKeyId="<your-access-key-id>"
                accessKey="<your-access-key>"
                logStore="<your-log-store-name>">
            <contextProperty name="time" layout="${longdate}" />
            <contextProperty name="level" layout="${level}" />
            <contextProperty name="sequence" layout="${sequenceId}" />
            <contextProperty name="message" layout="${message}" />
            <contextProperty name="exception" layout="${exception:format=ToString,StackTrace}" includeEmptyValue="false" />
        </target>
```

- Add Logger Rules
```xml
        <logger name="*" minlevel="Debug" writeTo="aliyun" />
```

See a [full example](demo/NLog.config) in the demo project.
