<!-- 8-12 吴昊泽 首页界面更新 -->
<template>
    <div class="container">
      <img class="background" src="@/assets/index_bg.png" alt="Background" />
      <div class="overlay">
        <router-link to="/login" class="link-button"><img src="@/assets/index_button.png" /></router-link>
        <img class="cover" src="@/assets/index_cover.png" alt="Cover" />
      </div>
    </div>
  </template>
  
  <style scoped lang="scss">
  .container {
    position: relative;  //相对定位
    width: 100vw;
    height: 100vh;
    overflow: hidden;
  }
  
  .background {
    width: 100%;
    height: 100%;
    object-fit: cover;  //确保全覆盖
    position: absolute;  //绝对定位
    top: 0;
    left: 0;
    z-index: 1;
  }
  
  .overlay {  //覆盖层
    position: relative;
    z-index: 2;  //覆盖层的层级
    display: flex;  //弹性布局
    flex-direction: column;  //垂直排列
    align-items: center;  //居中对齐
    justify-content: center;  //居中对齐
    height: 100%;  //高度为100%
  }
  .cover {
    width: 90vw;
    height: auto;
    position: absolute;
    top: 20%;
  }
  
  .link-button{
    position: absolute;  
    z-index: 3;
    display: flex;
    justify-content: center;
    align-items: center;
    top: 62%;
    left: 17%;
    img{
      width: 15vw;
      height: auto;
    }
  }
  
  </style>