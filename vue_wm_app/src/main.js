import { createApp } from 'vue'
import './style.css'
import './assets/main.scss'
import App from './App.vue'
import { gsap } from 'gsap';
import ElementPlus from 'element-plus'
import zhCn from 'element-plus/dist/locale/zh-cn.mjs'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue' //导入 ElementPlus 组件库中的所有图标

import store from '~/store' 

import {router} from './router'   // 从export default router 改为 export function router = createRouter({}) 需要添加 {}

const app = createApp(App) 

// 状态管理
app.use(store)

// 路由管理
app.use(router)

//注册 ElementPlus 组件库中的所有图标到全局 Vue 应用中
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
    app.component(key, component)
}
app.use(ElementPlus, { //将 ElementPlus 插件注册到 Vue 应用中
    locale: zhCn // 设置 ElementPlus 组件库的区域语言为中文简体
})

app.mount('#app')
