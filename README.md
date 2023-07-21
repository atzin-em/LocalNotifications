<!-- Improved compatibility of back to top link: See: https://github.com/atzin-em/LocalNotifications/pull/73 -->
<a name="readme-top"></a>
<!--
*** Thanks for checking out the Best-README-Template. If you have a suggestion
*** that would make this better, please fork the repo and create a pull request
*** or simply open an issue with the tag "enhancement".
*** Don't forget to give the project a star!
*** Thanks again! Now go create something AMAZING! :D
-->



<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">

  <h3 align="center">LocalNotifications</h3>

  <p align="center">
    Android push notifications for local network api server.
    <br />
    <br />
    <br />
    <a href="https://github.com/atzin-em/LocalNotifications/issues">Report Bug</a>
    ·
    <a href="https://github.com/atzin-em/LocalNotifications/issues">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#setup">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

<!-- [![Product Name Screen Shot][product-screenshot]](https://example.com) -->

This app is meant to assist in the utility of local IOT devices. Using no 3rd party servers, so long as your IOT device is able to send a POST request to an api server you should be able to receive notifications on your phone while on your local network. 

Use Cases:
* Getting alerts to certain IOT devices without needing to pay for proprietry 3rd party servers to recieve push notifications
* Enabling push notifications on homemade IOT devices
* Receiving push notifications from the API of some other service

<p align="right">(<a href="#readme-top">back to top</a>)</p>



### Built With


* [![.][C#]][C#-url]
* [![.][Xamarin]][Xamarin-url]
* [![.][AndroidX]][AndroidX-url]
* [![.][Python3]][Python3-url]


<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

What you will need:
* An API server on your local network
* An IOT type device with the ability to send a POST request
* An Android with 13.0 

### Setup

_This is a general setup for how to get utility from the application._

1. Download python API server script
2. Configure the script to return receive POST data from your IOT device/s
   ```py
   
   ```
3. Configure the script to return desired data when GET request is made
   ```py
   
   ```
4. Run the script on a network device (ideally this is a device that remains continuously on e.g. PiHole, OpenWRT Router)
5. Ensure the aforementioned network device has a static ip
6. Install the latest version of LocalNotifications from releases
   

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

Use this space to show useful examples of how a project can be used. Additional screenshots, code examples and demos work well in this space. You may also link to more resources.

_For more examples, please refer to the [Documentation](https://example.com)_

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap

- [ ] Add Changelog
- [ ] Add Custom Repeat Intervals
- [ ] Add Comparators for the values check of the notifications

See the [open issues](https://github.com/atzin-em/LocalNotifications/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* [Best README Template](https://github.com/othneildrew/Best-README-Template/blob/master/README.md)
* [Choose an Open Source License](https://choosealicense.com)
* [GitHub Emoji Cheat Sheet](https://www.webpagefx.com/tools/emoji-cheat-sheet)
* [Malven's Flexbox Cheatsheet](https://flexbox.malven.co/)
* [Malven's Grid Cheatsheet](https://grid.malven.co/)
* [Img Shields](https://shields.io)
* [GitHub Pages](https://pages.github.com)
* [Font Awesome](https://fontawesome.com)
* [React Icons](https://react-icons.github.io/react-icons/search)

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/atzin-em/LocalNotifications.svg?style=for-the-badge
[contributors-url]: https://github.com/atzin-em/LocalNotifications/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/atzin-em/LocalNotifications.svg?style=for-the-badge
[forks-url]: https://github.com/atzin-em/LocalNotifications/network/members
[stars-shield]: https://img.shields.io/github/stars/atzin-em/LocalNotifications.svg?style=for-the-badge
[stars-url]: https://github.com/atzin-em/LocalNotifications/stargazers
[issues-shield]: https://img.shields.io/github/issues/atzin-em/LocalNotifications.svg?style=for-the-badge
[issues-url]: https://github.com/atzin-em/LocalNotifications/issues
[license-shield]: https://img.shields.io/github/license/atzin-em/LocalNotifications.svg?style=for-the-badge
[license-url]: https://github.com/atzin-em/LocalNotifications/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/othneildrew
[product-screenshot]: images/screenshot.png

[C#]: https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white
[C#-url]: https://learn.microsoft.com/en-us/dotnet/csharp/
[Xamarin]: https://img.shields.io/badge/Xamarin-3199DC?style=for-the-badge&logo=xamarin&logoColor=white
[Xamarin-url]: https://dotnet.microsoft.com/en-us/apps/xamarin
[AndroidX]: https://img.shields.io/badge/AndroidX-3DDC84?style=for-the-badge&logo=android&logoColor=white
[AndroidX-url]: https://developer.android.com/jetpack/androidx
[Python3]: https://img.shields.io/badge/python3-3670A0?style=for-the-badge&logo=python&logoColor=ffdd54
[Python3-url]: https://www.python.org/
